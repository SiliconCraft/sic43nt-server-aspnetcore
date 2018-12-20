using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SIC43NT_Webserver.Utility.KeyStream
{
    public class Encrypt
    {
        private UInt32[] r = new UInt32[3];
        private UInt32[] s = new UInt32[3];
        
        public UInt32[] R
        {
            get
            {
                return r;
            }
            set
            {
                this.r = value;
            }
        }

        public UInt32[] S
        {
            get
            {
                return s;
            }
            set
            {
                this.s = value;
            }
        }
#if false
        public UInt32 getR(UInt32 index)
        {
            return r[index];
        }

        public void setR(UInt32 index, UInt32 data)
        {
            this.r[index] = data;
        }

        public UInt32 getS(UInt32 index)
        {
            return s[index];
        }

        public void setS(UInt32 index, UInt32 data)
        {
            this.s[index] = data;
        }
#endif
    }
}
