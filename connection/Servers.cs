using System.Net;
using System.Net.Sockets;

namespace SurvivalGameServer
{
    internal class Servers
    {
        private TCPServer tcpServer;
        private UDPServer udpServer;
        private bool isActive;

        private static Servers instance;
        private Servers()
        {
            StartServers();
        }
        public static Servers GetInstance()
        {
            if (instance == null)
            {
                instance = new Servers();
            }
            return instance;
        }

        private void StartServers()
        {
            if (isActive)
            {
                Globals.Logger.Write(Serilog.Events.LogEventLevel.Warning, "Servers allready started!");
                return;
            }

            try
            {
                tcpServer = new TCPServer(IPAddress.Any, Globals.TCP_PORT);
                udpServer = new UDPServer(IPAddress.Any, Globals.UDP_PORT);

                tcpServer.Start();                
                udpServer.Start();
                isActive = true;
                Globals.Logger.Write(Serilog.Events.LogEventLevel.Information, "started TCP and UDP servers");
            }
            catch (Exception ex)
            {
                Globals.Logger.Write(Serilog.Events.LogEventLevel.Error, $"error starting servers");
                Globals.Logger.Write(Serilog.Events.LogEventLevel.Error, ex.ToString());
            }
        }

        public void StopServers()
        {
            try
            {
                tcpServer.Stop();
                udpServer.Stop();
                Globals.Logger.Write(Serilog.Events.LogEventLevel.Information, "stopped TCP and UDP servers");
                isActive = false;
                instance = null;
            }
            catch (Exception ex)
            {
                Globals.Logger.Write(Serilog.Events.LogEventLevel.Error, $"error stopping servers");
                Globals.Logger.Write(Serilog.Events.LogEventLevel.Error, ex.ToString());
            }
        }

        public bool SendTCP(byte[] data, Guid id)
        {
            if (tcpServer.FindSession(id) != null)
            {
                return tcpServer.FindSession(id).SendAsync(data);
            }
            else
            {
                return false;
            }
        }

        public bool SendTCP(string data, Guid id)
        {
            if (tcpServer.FindSession(id) != null)
            {
                return tcpServer.FindSession(id).SendAsync(data);
            }
            else
            {
                return false;
            }
        }

        public bool SendTCP(byte[] data, byte[] key, Guid id, Globals.PacketCode code)
        {
            if (tcpServer.FindSession(id) != null)
            {
                Encryption.Encode(ref data, key, code);
                return tcpServer.FindSession(id).SendAsync(data);
            }
            else
            {
                return false;
            }
        }

        public bool SendUDP(byte[] data, byte[] key, EndPoint endpoint, Globals.PacketCode code)
        {
            //Console.WriteLine(string.Join('=', data));
            Encryption.Encode(ref data, key, code);
            //Console.WriteLine(string.Join('=', data));
            
            return udpServer.SendAsync(endpoint, data);
        }

        public bool SendUDPMovementPacketFromServer(MovementPacketFromServer packet, byte[] secretKey, EndPoint endpoint)
        {
            return SendUDP(ProtobufSchemes.SerializeProtoBuf(packet), secretKey, endpoint, Globals.PacketCode.MoveFromServer);
        }

        public bool SendTCPInitialPlayerData(PlayerDataInitial packet, byte[] secretKey, Guid id)
        {
            return SendTCP(ProtobufSchemes.SerializeProtoBuf(packet), secretKey, id, Globals.PacketCode.InitialPlayerData);
        }
    }
}
