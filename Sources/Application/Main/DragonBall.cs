using System;
using Microsoft.Extensions.Configuration;
using NRO_Server.Application.IO;
using NRO_Server.Application.Threading;
using NRO_Server.Logging;
using NRO_Server.DatabaseManager;

namespace NRO_Server.Application.Main
{
    public class DragonBall
    {
        private static bool keepRunning = true;
        static void Main(string[] args)
        {
            
            IServerLogger logger = new ServerLogger();
            var configBuilder = new ConfigurationBuilder().SetBasePath(ServerUtils.ProjectDir(""))
                .AddJsonFile("config.json");
            var configurationRoot = configBuilder.Build();

            DatabaseManager.Manager.CreateManager(configurationRoot);
            Server.Gi().StartServer(true, logger, configurationRoot, false);
            Console.CancelKeyPress += delegate(object sender, ConsoleCancelEventArgs e) {
                e.Cancel = true;
                DragonBall.keepRunning = false;
            };
            while (keepRunning)
            {
                var type = Console.ReadLine();
                if (type != null && type.Contains("baotri"))
                {
                    var time = 1;
                    try
                    {
                        time = Int32.Parse(type.Replace("baotri",""));
                    }
                    catch (Exception)
                    {
                        // ignored
                    }

                    if (Maintenance.Gi().IsStart)
                    {
                        logger.Print($"Server is Maintained, time Left: {Maintenance.Gi().TimeCount} minutes...");
                    }
                    else
                    {
                        Maintenance.Gi().Start(time);
                        logger.Print($"Server will be under Maintenance Later: {time} minutes...");
                    }
                    
                }
                else if (type == "restart")
                {
                    logger.Print("Server restarting...");
                    configBuilder = new ConfigurationBuilder().SetBasePath(ServerUtils.ProjectDir(""))
                        .AddJsonFile("config.json");
                    configurationRoot = configBuilder.Build();
                    DatabaseManager.Manager.CreateManager(configurationRoot);
                    Server.Gi().RestartServer();
                }
                else if (type == "shutdown")
                {
                    logger.Print("Server stopping...");
                    Server.Gi().StopServer();
                    break;
                }
                else
                {
                   Console.WriteLine("Not Found Action..."); 
                }
            }
            logger.Print("Server stopping...");
            Server.Gi().StopServer();
        }
    }
}