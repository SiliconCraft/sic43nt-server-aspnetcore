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

        public string flagTamper_result_color = "notavaliable_result";
        public string timeStamp_result_color = "notavaliable_result";
        public string rollingCode_result_color = "notavaliable_result";

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
            if (d is null)
            {

            }
            else
            {
                if (d.Length == 32)
                {
                    // Extract Content from Query String
                    uid = d.Substring(0, 14);
                    flagTamperTag = d.Substring(14, 2);
                    timeStampTag_str = d.Substring(16, 8);
                    timeStampTag_uint = UInt32.Parse(timeStampTag_str, System.Globalization.NumberStyles.HexNumber);
                    rollingCodeTag = d.Substring(24, 8);
                    
                    // Retrive content from table server from existing UID
                    tagAr = _serv.GetTagAccessRec("DemoSection", uid);
                    default_key = tagAr.SecretKey;
                    rollingCodeServer = KeyStream.stream(default_key, timeStampTag_str, 4);
                    timeStampServer_uint = (uint)tagAr.TimeStampServer;
                    timeStampServer_str = timeStampServer_uint.ToString("X8");

                    // Check the consistance of data from query string and table server 
                    result_agreement_check(tagAr);
                }
            }
        }
        private void result_agreement_check(TagAccessRec tar)
        {
            if (rollingCodeServer == rollingCodeTag)
            {
                rollingCode_result_color = "correct_result";
                if (timeStampServer_uint < timeStampTag_uint)
                {
                    timeStampDecision = "Rolling code updated";
                    // Update TimeStamp
                    tagAr.TimeStampServer = (int)timeStampTag_uint;
                    tagAr.SuccessCount++;
                    tagAr.SuccessLastDateTime = DateTime.Now;
                    timeStamp_result_color = "correct_result";
                }
                else
                {
                    timeStampDecision = "Rolling code reused";
                    // Update TimeStamp
                    tagAr.TimeStampFailCount++;
                    tagAr.TimeStampFailLastDateTime = DateTime.Now;
                    timeStamp_result_color = "incorrect_result";
                }
            }
            else
            {
                tagAr.RollingCodeFailCount++;
                tagAr.RollingCodeFailLastDateTime = DateTime.Now;
                rollingCode_result_color = "incorrect_result";

            }
            _serv.UpdateTagAccessRec(tagAr);

            /*---- Time Stamp Counting Decision ----*/
            if (timeStampServer_str == "N/A")
            { 
                timeStampDecision = "N/A";
            }
            else
            {

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
