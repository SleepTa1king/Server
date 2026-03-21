using System;
using System.Diagnostics;
using System.Threading;
using System.Collections.Generic;

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
                Console.WriteLine($"[Server] Client connected: {client.RemoteEndPoint}");
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
                    case SocketEvent.cs_input:
                        frameSync.CollectInput(client, data.Data);
                        break;
                }
            };

            frameSync.Start();
            Console.WriteLine("[Server] FrameSync started. Press 'S' to restart, 'K' to kick all, 'ESC' to exit.");

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
                    if (key == ConsoleKey.S) { frameSync.Start(); Console.WriteLine("[Server] FrameSync RESTARTED"); }
                    if (key == ConsoleKey.K) { server.KickOutAll(); Console.WriteLine("[Server] Kicked all clients"); }
                }

                // Sleep a bit to prevent 100% CPU usage
                Thread.Sleep(1);
            }

            frameSync.Stop();
            server.Close();
            Console.WriteLine("[Server] Server stopped.");
        }
    }
}
