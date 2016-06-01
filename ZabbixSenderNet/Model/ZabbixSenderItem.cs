
namespace ZabbixSenderNet.Model
{
    public class ZabbixSenderItem
    {
        #region Properties
        public string Host { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="ZabbixSenderItem"/> class.
        /// </summary>
        public ZabbixSenderItem()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZabbixSenderItem"/> class.
        /// </summary>
        /// <param name="host">The host.</param>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public ZabbixSenderItem(string host, string key, string value)
        {
            Host = host;
            Key = key;
            Value = value;
        }
        #endregion
    }
}
