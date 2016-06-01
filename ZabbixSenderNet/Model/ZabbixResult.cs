using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace ZabbixSenderNet.Model
{
    public class ZabbixResult
    {
        #region Properties
        [JsonProperty("response")]
        public string Response { get; set; }
        [JsonProperty("info")]
        public string InformationText { get; set; }

        private ZabbixResultInfo zabbixResultInfo;
        public ZabbixResultInfo ZabbixResultInfo
        {
            get
            {
                if (zabbixResultInfo == null)
                    zabbixResultInfo = ParseInformationText();

                return zabbixResultInfo;

            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is success.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is success; otherwise, <c>false</c>.
        /// </value>
        public bool IsSuccess
        {
            get
            {
                return ZabbixResultInfo.Success();
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Parses the information.
        /// </summary>
        /// <returns></returns>
        private ZabbixResultInfo ParseInformationText()
        {
            //Example : of line to process
            //processed: 1; failed: 0; total: 1; seconds spent: 0.000053\
            string[] split = Regex.Split(InformationText, "[^0-9\\.]+");

            int processed = 0, failed = 0, total = 0, spentSeconds = 0;

            int.TryParse(split[1], out processed);
            int.TryParse(split[2], out failed);
            int.TryParse(split[3], out total);
            int.TryParse(split[4], out spentSeconds);

            return new ZabbixResultInfo(processed, failed, total, spentSeconds);
        }
        #endregion
    }

    public class ZabbixResultInfo
    {
        #region Properties
        public int Processed { get; set; }
        public int Failed { get; set; }
        public int Total { get; set; }
        public float SpentSeconds { get; set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="ZabbixResultInfo"/> class.
        /// </summary>
        /// <param name="processed">The processed.</param>
        /// <param name="failed">The failed.</param>
        /// <param name="total">The total.</param>
        /// <param name="spentSeconds">The spent seconds.</param>
        public ZabbixResultInfo(int processed, int failed, int total, int spentSeconds)
        {
            Processed = processed;
            Failed = failed;
            Total = total;
            SpentSeconds = spentSeconds;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Return true if all went alright
        /// </summary>
        /// <returns></returns>
        public bool Success()
        {
            return Processed == Total;
        }
        #endregion

    }
}
