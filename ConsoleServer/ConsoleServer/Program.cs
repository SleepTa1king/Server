using System;
using System.Diagnostics;
using System.Threading;
using System.Collections.Generic;
using System.Net.Sockets;

namespace Server
{
    class Program
    {
        static void Main(string[] args)
        {
            string ip = "127.0.0.1";
            int port = 6854;

            Console.WriteLine($"[Server] Starting standalone server on {ip}:{port}...");

            SocketServer server = new SocketServer(ip, port);
            FrameSyncManager frameSync = new FrameSyncManager(server);

            server.OnConnect += (client) =>
            {
                Console.WriteLine($"[Server] Client connected: {client.RemoteEndPoint}. Waiting for login...");
            };

            server.OnDisconnect += (client) =>
            {
                Console.WriteLine("[Server] Client disconnected");
                frameSync.RemoveClient(client);
            };

            server.OnReceive += (client, data) =>
            {
                switch ((SocketEvent)data.Type)
                {
                    case SocketEvent.cs_login:
                        // 收到客户端上报的哈希 ID
                        var loginReq = ProtoHelper.Deserialize<CSLogin>(data.Data);
                        int hashId = loginReq.PlayerId;
                        
                        // 回传确认消息
                        var loginAck = new SCLogin { PlayerId = hashId };
                        server.Send(client, (ushort)SocketEvent.sc_login, ProtoHelper.Serialize(loginAck));
                        
                        Console.WriteLine($"[Server] Client logged in with Hash ID: {hashId}");
                        break;

                    case SocketEvent.cs_input:
                        frameSync.CollectInput(client, data.Data);
                        break;
                }
            };

            frameSync.Start();
            Console.WriteLine("[Server] FrameSync started. ESC to exit.");

            Stopwatch sw = new Stopwatch();
            sw.Start();
            long lastTicks = sw.ElapsedTicks;

            bool running = true;
            while (running)
            {
                long currentTicks = sw.ElapsedTicks;
                float deltaTime = (float)(currentTicks - lastTicks) / Stopwatch.Frequency;
                lastTicks = currentTicks;

                server.Tick(deltaTime);
                frameSync.Tick(deltaTime);

                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey(true).Key;
                    if (key == ConsoleKey.Escape) running = false;
                }

                Thread.Sleep(1);
            }

            frameSync.Stop();
            server.Close();
            Console.WriteLine("[Server] Server stopped.");
        }
    }
}
