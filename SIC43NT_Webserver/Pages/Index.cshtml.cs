using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SIC43NT_Webserver.Utility.KeyStream;

namespace SIC43NT_Webserver.Pages
{
    public class IndexModel : PageModel
    {
        public string default_key = "N/A";
        public string uid = "N/A";

        public string flagTamperTag = "-";
        public string timeStampTag_uint = "-";
        public string timeStampTag_str = "N/A";
        public string rollingCodeTag = "-";

        public string flagTamperServer = "N/A";
        public uint timeStampServer_uint;
        public string timeStampServer_str = "N/A";
        public string rollingCodeServer = "N/A";
        public string rlc = "";

        public string timeStampDecision = "N/A";
        public string flagTamperDecision = "N/A";
        public string rollingCodeDecision = "N/A";

        public void OnGet(string d)
        {
            if (d is null)
            {

            }
            else
            {
                if (d.Length == 32)
                {
                    uid = d.Substring(0, 14);
                    flagTamperTag = d.Substring(14, 2);
                    timeStampTag_str = d.Substring(16, 8);
                    timeStampTag_uint = UInt32.Parse(timeStampTag_str, System.Globalization.NumberStyles.HexNumber).ToString();
                    rollingCodeTag = d.Substring(24, 8);
                    default_key = "FFFFFF" + uid;
                    rollingCodeServer = KeyStream.stream(default_key, timeStampTag_str, 4);
                    result_agreement_check();
                }
            }
        }
        private void result_agreement_check()
        {
            /*---- Time Stamp Counting Decision ----*/
            if (timeStampServer_str == "N/A")
            {
                timeStampDecision = "N/A";
            }
            else
            {
                if (timeStampServer_uint < UInt32.Parse(timeStampTag_uint))
                {
                    timeStampDecision = "Rolling code updated";
                }
                else
                {
                    timeStampDecision = "Rolling code reused";
                }
            }

            /*---- Rolling Code Counting Decision ----*/
            if (rollingCodeServer == rollingCodeTag)
            {
                rollingCodeDecision = "Correct";
            }
            else
            {
                if (flagTamperTag == "AA")
                {
                    rlc = KeyStream.stream(default_key, timeStampTag_str, 12);
                    rollingCodeServer = rlc.Substring(16, 8);

                    if (rollingCodeServer == rollingCodeTag)
                    {
                        rollingCodeDecision = "Correct";
                    }
                    else
                    {
                        rollingCodeDecision = "Incorrect";
                    }
                }
                else
                {
                    rollingCodeDecision = "Incorrect";
                }
            }
        }
    }
}
