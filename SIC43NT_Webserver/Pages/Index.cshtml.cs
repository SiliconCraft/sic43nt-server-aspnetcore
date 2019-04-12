using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SIC43NT_Webserver.Utility.KeyStream;
using SIC43NT_Webserver.TableStorage;

namespace SIC43NT_Webserver.Pages
{
    public class IndexModel : PageModel
    {
        public string default_key;
        public string uid;

        public string flagTamperTag;
        public uint timeStampTag_uint;
        public string timeStampTag_str;
        public string rollingCodeTag;

        public string flagTamperServer = "N/A";
        public uint timeStampServer_uint;
        public string timeStampServer_str = "N/A";
        public string rollingCodeServer = "N/A";

        public string timeStampDecision = "N/A";
        public string flagTamperDecision = "N/A";
        public string rollingCodeDecision = "N/A";
        public TagAccessRec tagAr;
        private IAzureTableStorage _serv;

        public IndexModel(IAzureTableStorage serv)
        {
            _serv = serv;
        }

        public void OnGet(string d)
        {
            tagAr = _serv.GetTagAccessRec("DemoSection", "1234");
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
                    timeStampTag_uint = UInt32.Parse(timeStampTag_str, System.Globalization.NumberStyles.HexNumber);
                    rollingCodeTag = d.Substring(24, 8);
                    //default_key = "FFFFFF" + uid;
                    default_key = tagAr.SecretKey;
                    rollingCodeServer = KeyStream.stream(default_key, timeStampTag_str, 4);
                    timeStampServer_uint = (uint)tagAr.TimeStampServer;
                    timeStampServer_str = timeStampServer_uint.ToString("X8");
                    result_agreement_check();
                    // Update TimeStamp
                    _serv.UpdateTagAccessRec(tagAr);    
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
                if (timeStampServer_uint < timeStampTag_uint)
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
                rollingCodeDecision = "Incorrect";
            }
        }
    }
}
