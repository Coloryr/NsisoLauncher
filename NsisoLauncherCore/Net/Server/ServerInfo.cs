using Heijden.Dns.Portable;
using Heijden.DNS;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NsisoLauncherCore.Net.Server
{
    class ServerInfo
    {
        /// <summary>
        /// 服务器IP地址
        /// </summary>
        public string ServerAddress { get; set; }

        /// <summary>
        /// 服务器端口
        /// </summary>
        public ushort ServerPort { get; set; }

        /// <summary>
        /// 服务器名称
        /// </summary>
        public string ServerName { get; set; }

        /// <summary>
        /// 获取服务器MOTD
        /// </summary>
        public string MOTD { get; private set; }

        /// <summary>
        /// 获取服务器的最大玩家数量
        /// </summary>
        public int MaxPlayerCount { get; private set; }

        /// <summary>
        /// 获取服务器的在线人数
        /// </summary>
        public int CurrentPlayerCount { get; private set; }

        /// <summary>
        /// 获取服务器版本号
        /// </summary>
        public int ProtocolVersion { get; private set; }

        /// <summary>
        /// 获取服务器游戏版本
        /// </summary>
        public string GameVersion { get; private set; }

        /// <summary>
        /// 获取服务器详细的服务器信息JsonResult
        /// </summary>
        public string JsonResult { get; private set; }

        /// <summary>
        /// 获取服务器Forge信息（如果可用）
        /// </summary>
        public ForgeInfo ForgeInfo { get; private set; }

        /// <summary>
        /// 获取服务器在线玩家的名称（如果可用）
        /// </summary>
        public List<string> OnlinePlayersName { get; private set; }

        /// <summary>
        /// 获取此次连接服务器的延迟(ms)
        /// </summary>
        public long Ping { get; private set; }

        /// <summary>
        /// Icon DATA
        /// </summary>
        public byte[] IconData { get; set; }

        /// <summary>
        /// 连接状态
        /// </summary>
        public StateType State { get; set; }

        /// <summary>
        /// 获取与特定格式代码相关联的颜色代码
        /// </summary>

        public enum StateType
        {
            GOOD,
            NO_RESPONSE,
            BAD_CONNECT,
            EXCEPTION
        }

        public ServerInfo(string ip, ushort port)
        {
            this.ServerAddress = ip;
            this.ServerPort = port;
        }

        public ServerInfo(Modules.Server info)
        {
            this.ServerAddress = info.Address;
            this.ServerPort = info.Port;
        }

        public void StartGetServerInfo()
        {
            try
            {
                // Some code source form:
                // Minecraft Client v1.9.0 for Minecraft 1.4.6 to 1.9.0 - By ORelio under CDDL-1.0
                // wiki.vg

                TcpClient tcp = null;

                try
                {
                    tcp = new TcpClient(this.ServerAddress, this.ServerPort);
                }
                catch (SocketException)
                {
                    RecordSRV result = (RecordSRV)new Resolver().Query("_minecraft._tcp." + this.ServerAddress, QType.SRV).Result.Answers?.FirstOrDefault()?.RECORD;
                    if (result != null)
                    {
                        tcp = new TcpClient(result.TARGET, result.PORT);
                        this.ServerAddress = result.TARGET;
                        this.ServerPort = result.PORT;
                    }
                    else
                    {
                        this.State = StateType.BAD_CONNECT;
                        return;
                    }
                }

                try
                {
                    tcp.ReceiveBufferSize = 1024 * 1024;

                    byte[] packet_id = ProtocolHandler.getVarInt(0);
                    byte[] protocol_version = ProtocolHandler.getVarInt(-1);
                    byte[] server_adress_val = Encoding.UTF8.GetBytes(this.ServerAddress);
                    byte[] server_adress_len = ProtocolHandler.getVarInt(server_adress_val.Length);
                    byte[] server_port = BitConverter.GetBytes((ushort)this.ServerPort); Array.Reverse(server_port);
                    byte[] next_state = ProtocolHandler.getVarInt(1);
                    byte[] packet2 = ProtocolHandler.concatBytes(packet_id, protocol_version, server_adress_len, server_adress_val, server_port, next_state);
                    byte[] tosend = ProtocolHandler.concatBytes(ProtocolHandler.getVarInt(packet2.Length), packet2);

                    byte[] status_request = ProtocolHandler.getVarInt(0);
                    byte[] request_packet = ProtocolHandler.concatBytes(ProtocolHandler.getVarInt(status_request.Length), status_request);

                    tcp.Client.Send(tosend, SocketFlags.None);

                    tcp.Client.Send(request_packet, SocketFlags.None);
                    ProtocolHandler handler = new ProtocolHandler(tcp);
                    int packetLength = handler.readNextVarIntRAW();
                    if (packetLength > 0)
                    {
                        List<byte> packetData = new List<byte>(handler.readDataRAW(packetLength));
                        if (ProtocolHandler.readNextVarInt(packetData) == 0x00) //Read Packet ID
                        {
                            string result = ProtocolHandler.readNextString(packetData); //Get the Json data
                            this.JsonResult = result;
                            SetInfoFromJsonText(result);
                        }
                    }

                    byte[] ping_id = ProtocolHandler.getVarInt(1);
                    byte[] ping_content = BitConverter.GetBytes((long)233);
                    byte[] ping_packet = ProtocolHandler.concatBytes(ping_id, ping_content);
                    byte[] ping_tosend = ProtocolHandler.concatBytes(ProtocolHandler.getVarInt(ping_packet.Length), ping_packet);

                    try
                    {
                        tcp.ReceiveTimeout = 1000;

                        Stopwatch pingWatcher = new Stopwatch();

                        pingWatcher.Start();
                        tcp.Client.Send(ping_tosend, SocketFlags.None);

                        int pingLenghth = handler.readNextVarIntRAW();
                        pingWatcher.Stop();
                        if (pingLenghth > 0)
                        {
                            List<byte> packetData = new List<byte>(handler.readDataRAW(pingLenghth));
                            if (ProtocolHandler.readNextVarInt(packetData) == 0x01) //Read Packet ID
                            {
                                long content = ProtocolHandler.readNextByte(packetData); //Get the Json data
                                if (content == 233)
                                {
                                    this.Ping = pingWatcher.ElapsedMilliseconds;
                                }
                            }
                        }
                    }
                    catch (Exception)
                    {
                        this.Ping = 0;
                    }

                }
                catch (SocketException)
                {
                    this.State = StateType.NO_RESPONSE;
                }
                tcp.Close();
            }
            catch (SocketException)
            {
                this.State = StateType.BAD_CONNECT;
            }
            catch (Exception)
            {
                this.State = StateType.EXCEPTION;
            }
        }

        public async Task StartGetServerInfoAsync()
        {
            await Task.Factory.StartNew(() =>
            {
                StartGetServerInfo();
            });
        }

        private void SetInfoFromJsonText(string JsonText)
        {
            try
            {
                JsonText = ClearColor(JsonText);
                if (!string.IsNullOrEmpty(JsonText) && JsonText.StartsWith("{") && JsonText.EndsWith("}"))
                {
                    JObject jsonData = JObject.Parse(JsonText);

                    if (jsonData.ContainsKey("version"))
                    {
                        JObject versionData = (JObject)jsonData["version"];
                        this.GameVersion = versionData["name"].ToString();
                        this.ProtocolVersion = int.Parse(versionData["protocol"].ToString());
                    }

                    if (jsonData.ContainsKey("players"))
                    {
                        JObject playerData = (JObject)jsonData["players"];
                        this.MaxPlayerCount = int.Parse(playerData["max"].ToString());
                        this.CurrentPlayerCount = int.Parse(playerData["online"].ToString());
                        if (playerData.ContainsKey("sample"))
                        {
                            this.OnlinePlayersName = new List<string>();
                            foreach (JObject name in playerData["sample"])
                            {
                                if (name.ContainsKey("name"))
                                {
                                    string playername = name["name"].ToString();
                                    this.OnlinePlayersName.Add(playername);
                                }
                            }
                        }
                    }

                    if (jsonData.ContainsKey("description"))
                    {
                        JToken descriptionData = jsonData["description"];
                        if (descriptionData.Type == JTokenType.String)
                        {
                            this.MOTD = descriptionData.ToString();
                        }
                        else if (descriptionData.Type == JTokenType.Object)
                        {
                            JObject descriptionDataObj = (JObject)descriptionData;
                            if (descriptionDataObj.ContainsKey("extra"))
                            {
                                foreach (var item in descriptionDataObj["extra"])
                                {
                                    string text = item["text"].ToString();
                                    if (!string.IsNullOrWhiteSpace(text))
                                    {
                                        this.MOTD += text;
                                    }
                                }
                            }
                            else if (descriptionDataObj.ContainsKey("text"))
                            {
                                this.MOTD = descriptionDataObj["text"].ToString();
                            }
                        }
                    }

                    // Check for forge on the server.
                    if (jsonData.ContainsKey("modinfo") && jsonData["modinfo"].Type == JTokenType.Object)
                    {
                        JObject modData = (JObject)jsonData["modinfo"];
                        if (modData.ContainsKey("type") && modData["type"].ToString() == "FML")
                        {
                            this.ForgeInfo = new ForgeInfo(modData);
                            if (!this.ForgeInfo.Mods.Any())
                            {
                                this.ForgeInfo = null;
                            }
                        }
                    }

                    if (jsonData.ContainsKey("favicon"))
                    {
                        try
                        {
                            string datastring = jsonData["favicon"].ToString();
                            byte[] arr = Convert.FromBase64String(datastring.Replace("data:image/png;base64,", ""));
                            this.IconData = arr;
                        }
                        catch
                        {
                            this.IconData = null;
                        }
                    }

                    this.State = StateType.GOOD;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private string ClearColor(string str)
        {
            return str
                .Replace("§0", "").Replace("§a", "")
                .Replace("§1", "").Replace("§b", "")
                .Replace("§2", "").Replace("§c", "")
                .Replace("§3", "").Replace("§d", "")
                .Replace("§4", "").Replace("§e", "")
                .Replace("§5", "").Replace("§f", "")
                .Replace("§6", "").Replace("§n", "")
                .Replace("§7", "").Replace("§m", "")
                .Replace("§8", "").Replace("§k", "")
                .Replace("§9", "").Replace("§r", "")
                .Replace("§l", "");
        }
    }
}
