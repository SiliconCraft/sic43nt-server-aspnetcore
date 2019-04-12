using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace SIC43NT_Webserver.TableStorage
{
    public class TagAccessRec : TableEntity
    {
        public TagAccessRec()
        {

        }

        public String TableSection { get; set; }
        public String UID { get; set; }
        public String SecretKey { get; set; }
        public String RollingCodeServer { get; set; }
        public Int32 TimeStampServer { get; set; }
        public Int32 RollingCodeFailCount { get; set; }
        public Int32 TimeStampFailCount { get; set; }
        public Int32 SuccessCount { get; set; }
        public DateTime RollingCodeFailLastDateTime { get; set; }
        public DateTime TimeStampFailLastDateTime { get; set; }
        public DateTime SuccessLastDateTime { get; set; }
    }
}
