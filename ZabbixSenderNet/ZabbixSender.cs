using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using ZabbixSenderNet.Extension;
using ZabbixSenderNet.Model;

namespace ZabbixSenderNet
{
    public class ZabbixSender
    {
        #region Properties
        private const int DefaultTimeOut = 3000;
        //implementing the Zabbix Protocol https://www.zabbix.com/documentation/1.8/protocols
        private static byte[] Header = Encoding.ASCII.GetBytes("ZBXD\x01");

        public string Host { get; set; }
        public int Port { get; set; }
        public int ConnectionTimeout { get; set; }
        public int SocketTimeout { get; set; }
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="ZabbixSender"/> class.
        /// </summary>
        /// <param name="host">The host.</param>
        /// <param name="port">The port.</param>
        public ZabbixSender(String host, int port)
            : this(host, port, DefaultTimeOut, DefaultTimeOut)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZabbixSender"/> class.
        /// </summary>
        /// <param name="host">The host.</param>
        /// <param name="port">The port.</param>
        /// <param name="connectionTimeout">The connection timeout.</param>
        /// <param name="socketTimeout">The socket timeout.</param>
        public ZabbixSender(String host, int port, int connectionTimeout, int socketTimeout)
        {
            Host = host;
            Port = port;
            ConnectionTimeout = connectionTimeout;
            SocketTimeout = socketTimeout;
        }
        #endregion

        #region Methods

        /// <summary>
        /// Sends the specified host.
        /// </summary>
        /// <param name="host">The host.</param>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public ZabbixResult Send(string host, string key, string value)
        {
            return Send(new List<ZabbixSenderItem> { new ZabbixSenderItem(host, key, value) });
        }

        /// <summary>
        /// Sends the specified items.
        /// </summary>
        /// <param name="items">The items.</param>
        /// <returns></returns>
        public ZabbixResult Send(List<ZabbixSenderItem> items)
        {
            var jsonObject = new
            {
                request = "sender data",
                data = items
            };

            return Send(JsonConvert.SerializeObject(jsonObject, new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            }));
        }

        /// <summary>
        /// Sends the specified json message.
        /// </summary>
        /// <param name="jsonMessage">The json message.</param>
        /// <returns></returns>
        protected ZabbixResult Send(string jsonMessage)
        {
            ZabbixResult result = new ZabbixResult();

            try
            {
                byte[] length = BitConverter.GetBytes((long)jsonMessage.Length);
                byte[] jsonData = Encoding.ASCII.GetBytes(jsonMessage);

                //initialize array of Byte
                byte[] all = new byte[Header.Length + length.Length + jsonData.Length];

                System.Buffer.BlockCopy(Header, 0, all, 0, Header.Length);
                System.Buffer.BlockCopy(length, 0, all, Header.Length, length.Length);
                System.Buffer.BlockCopy(jsonData, 0, all, Header.Length + length.Length, jsonData.Length);

                using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
                {
                    socket.ReceiveTimeout = SocketTimeout;
                    socket.Connect(Host, Port, TimeSpan.FromSeconds(ConnectionTimeout));

                    // Send To Zabbix
                    socket.Send(all);

                    result = GetSocketResponse(socket);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return result;
        }

        /// <summary>
        /// Gets the socket response.
        /// </summary>
        /// <param name="socket">The socket.</param>
        /// <returns></returns>
        /// <exception cref="Exception">
        /// Invalid response
        /// or
        /// Invalid response
        /// </exception>
        private ZabbixResult GetSocketResponse(Socket socket)
        {
            //Receive Response Header
            byte[] responseHeader = new byte[5];
            GetSocketResponse(socket, responseHeader);

            if ("ZBXD\x01" != Encoding.ASCII.GetString(responseHeader, 0, responseHeader.Length))
                throw new Exception("Invalid response");

            // Receive Data Length
            var responseDataLength = new byte[8];
            GetSocketResponse(socket, responseDataLength);
            int dataLength = BitConverter.ToInt32(responseDataLength, 0);

            if (dataLength == 0)
                throw new Exception("Invalid response");

            // Receive Response Message
            var responseMessage = new byte[dataLength];
            GetSocketResponse(socket, responseMessage);

            string jsonResponse = Encoding.ASCII.GetString(responseMessage, 0, responseMessage.Length);

            return JsonConvert.DeserializeObject<ZabbixResult>(jsonResponse, new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });
        }

        /// <summary>
        /// Gets the socket response.
        /// </summary>
        /// <param name="socket">The socket.</param>
        /// <param name="buffer">The buffer.</param>
        private void GetSocketResponse(Socket socket, byte[] buffer)
        {
            int received = 0;
            int size = buffer.Length;
            int offset = 0;
            do
            {
                try
                {
                    received += socket.Receive(buffer, offset + received, size - received, SocketFlags.None);
                }
                catch (SocketException ex)
                {
                    if (ex.SocketErrorCode == SocketError.WouldBlock ||
                        ex.SocketErrorCode == SocketError.IOPending ||
                        ex.SocketErrorCode == SocketError.NoBufferSpaceAvailable)
                    {
                        // socket buffer is probably empty, wait and try again
                        System.Threading.Thread.Sleep(ConnectionTimeout);
                    }
                    else
                        throw ex;
                }
            } while (received < size);
        }
        #endregion
    }
}
