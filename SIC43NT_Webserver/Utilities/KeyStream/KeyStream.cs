using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace SIC43NT_Webserver.Utility.KeyStream
{
    public static class KeyStream
    {
        private static UInt32[] R_MASK = new UInt32[3]      { 0x1d5363d5, 0x415a0aac, 0x0000d2a8};  /* Initialise the feedback mask associated with register R */
        /* Feedback mask associated with the register R */
        private static UInt32[] COMP0 = new UInt32[3]       { 0x6aa97a30, 0x7942a809, 0x00003fea};  /* Initialise COMP0 */
        /* Input mask associated with register S */
        private static UInt32[] COMP1 = new UInt32[3]       { 0xdd629e9a, 0xe3a21d63, 0x00003dd7 }; /* Initialise COMP1 */
        /* Second input mask associated with register S */
        private static UInt32[] S_MASK0 = new UInt32[3]     { 0x9ffa7faf, 0xaf4a9381, 0x00005802 }; /* Initialise the feedback masks associated with register S */
        /* Feedback mask associated with the register S for clock control bit = 0 */
        private static UInt32[] S_MASK1 = new UInt32[3]     { 0x4c8cb877, 0x4911b063, 0x0000c52b }; /* Initialise the feedback masks associated with register S */
        /* Feedback mask associated with the register S for clock control bit = 1 */

        private static void clockR(Encrypt encrypt, UInt32 inputBit, UInt32 controlBit)
        {
            UInt32 feedbackBit;
            /* r_79 ^ input bit */
            UInt32 carry0, carry1;
            /* Respectively, carry from R[0] into R[1] and carry from R[1] into R[2] */
            UInt32 r0 = encrypt.R[0];
            UInt32 r1 = encrypt.R[1];
            UInt32 r2 = encrypt.R[2];

            /* Initialise the variables */
            feedbackBit = ((r2 >> 15) & 1) ^ inputBit;
            carry0 = (r0 >> 31) & 1;
            carry1 = (r1 >> 31) & 1;

            if (controlBit != 0)
            {
                /* Shift and xor */
                r0 ^= (r0 << 1);
                r1 ^= (r1 << 1) ^ carry0;
                r2 ^= (r2 << 1) ^ carry1;
            }
            else
            {
                /* Shift only */
                r0 = (r0 << 1);
                r1 = (r1 << 1) ^ carry0;
                r2 = (r2 << 1) ^ carry1;
            }

            /* Implement feedback into the various register stages */
            if (feedbackBit != 0)
            {
                r0 ^= R_MASK[0];
                r1 ^= R_MASK[1];
                r2 ^= R_MASK[2];
            }

            encrypt.R[0] = r0;
            encrypt.R[1] = r1;
            encrypt.R[2] = r2;
        }

        private static void clockS(Encrypt encrypt, UInt32 inputBit, UInt32 controlBit)
        {
            UInt32 feedbackBit;
            /* s_79 ^ input bit */
            UInt32 carry0, carry1;
            /* Respectively, carry from S[0] into S[1] and carry from S[1] into S[2] */
            UInt32 s0 = encrypt.S[0];
            UInt32 s1 = encrypt.S[1];
            UInt32 s2 = encrypt.S[2];

            /* Compute the feedback and two carry bits */
            feedbackBit = ((s2 >> 15) & 1) ^ inputBit;
            carry0 = (s0 >> 31) & 1;
            carry1 = (s1 >> 31) & 1;
            /* Derive "s hat" according to the MICKEY v 0.4 specification */
            s0 = (s0 << 1) ^ ((s0 ^ COMP0[0]) & ((s0 >> 1) ^ (s1 << 31) ^ COMP1[0]) & 0xfffffffe);
            s1 = (s1 << 1) ^ ((s1 ^ COMP0[1]) & ((s1 >> 1) ^ (s2 << 31) ^ COMP1[1])) ^ carry0;
            s2 = (s2 << 1) ^ ((s2 ^ COMP0[2]) & ((s2 >> 1) ^ COMP1[2]) & 0x7fff) ^ carry1;

            /* Apply suitable feedback from s_79 */
            if (feedbackBit != 0)
            {
                if (controlBit != 0)
                {
                    s0 ^= S_MASK1[0];
                    s1 ^= S_MASK1[1];
                    s2 ^= S_MASK1[2];
                }
                else
                {
                    s0 ^= S_MASK0[0];
                    s1 ^= S_MASK0[1];
                    s2 ^= S_MASK0[2];
                }
            }

            encrypt.S[0] = s0;
            encrypt.S[1] = s1;
            encrypt.S[2] = s2;
        }

        private static UInt32 clockKg(Encrypt encrypt, UInt32 mixing, UInt32 inputBit)
        {
            UInt32 keyStreamBit;
            UInt32 controlBitR;
            UInt32 controlBitS;

            UInt32 r0 = encrypt.R[0];
            UInt32 r1 = encrypt.R[1];
            UInt32 s0 = encrypt.S[0];
            UInt32 s1 = encrypt.S[1];

            keyStreamBit = (r0 ^ s0) & 1;
            controlBitR = ((s0 >> 27) ^ (r1 >> 21)) & 1;
            controlBitS = ((s1 >> 21) ^ (r0 >> 26)) & 1;

            if (mixing != 0)
            {
                clockR(encrypt, ((s1 >> 8) & 1) ^ inputBit, controlBitR);
            }
            else
            {
                clockR(encrypt, inputBit, controlBitR);
            }

            clockS(encrypt, inputBit, controlBitS);

            return keyStreamBit;
        }

        private static void setup(Encrypt encrypt, String key, String iv)
        {
            UInt32 ivkeyBit;

            uint keySize = (uint)key.Length;
            uint ivSize = (uint)iv.Length;

            /* Initialise R and S to all zeros */
            for (uint i = 0; i < 3; ++i)
            {
                encrypt.R[i] = 0;
                encrypt.S[i] = 0;
            }

            /* Load in IV */
            for (int i = 0; i < ivSize; ++i)
            {
                ivkeyBit = ((uint)iv[i] - '0') & 1; /* Adopt usual, perverse, labelling order */
                clockKg(encrypt, 1, ivkeyBit);
            }

            /* Load in K */
            for (int i = 0; i < keySize; ++i)
            {
                ivkeyBit = ((uint)key[i] - '0') & 1; /* Adopt usual, perverse, labelling order */
                clockKg(encrypt, 1, ivkeyBit);
            }

            /* Preclock */
            for (UInt32 i = 0; i < 80; ++i)
            {
                clockKg(encrypt, 1, 0);
            }
        }

        public static String stream(String key, String iv, UInt32 length)
        {
            UInt32 t_keystream;
            String resource = "";
            Encrypt encrypt = new Encrypt();
            String keyReverse;
            String ivReverse;

            {
                byte[] keyBytes = BigInteger.Parse("80" + key, NumberStyles.HexNumber).ToByteArray();
                string keyBin = string.Empty;
                for (int cnt = 0; cnt < keyBytes.Length - 1; cnt++)
                {
                    keyBin += Convert.ToString(keyBytes[keyBytes.Length - cnt - 2], 2).PadLeft(8, '0');
                }
                char[] charArray = keyBin.ToCharArray();
                Array.Reverse(charArray);
                keyReverse = new string(charArray);
                Console.WriteLine(keyBin);
            }

            {
                byte[] ivBytes = BigInteger.Parse("80" + iv, NumberStyles.HexNumber).ToByteArray();
                string ivBin = string.Empty;
                for (int cnt = 0; cnt < ivBytes.Length - 1; cnt++)
                {
                    ivBin += Convert.ToString(ivBytes[ivBytes.Length - cnt - 2], 2).PadLeft(8, '0');
                }
                char[] charArray = ivBin.ToCharArray();
                Array.Reverse(charArray);
                ivReverse = new string(charArray);
                Console.WriteLine(ivBin);
            }

            setup(encrypt, keyReverse, ivReverse);

            for (UInt32 i = 0; i < length; ++i)
            {
                t_keystream = 0;

                for (UInt32 j = 0; j < 8; ++j)
                {
                    t_keystream ^= clockKg(encrypt, 0, 0) << (int)(7 - j);
                }

                //resource += String.format(Locale.US, "%02X", keystream);
                resource = resource + t_keystream.ToString("X2");
            }

            return resource;
        }
    }
}
