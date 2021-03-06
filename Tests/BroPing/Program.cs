﻿using System;
using System.Threading;
using BroccoliSharp;

namespace BroPing
{
    class Program
    {
        static int Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Usage:");
                Console.WriteLine("    BroPing host:port");
                return 1;
            }

            try
            {
                string hostName = args[0];

                Console.WriteLine("Attempting to establish Bro connection to \"{0}\"...", hostName);

                // Create the connection object
                using (BroConnection connection = new BroConnection(hostName))
                {
                    // Register to receive the pong event
                    connection.RegisterForEvent("pong", e =>
                    {
                        BroValue[] values = e.Parameters;
                        DateTime src_time = values[0];
                        DateTime dst_time = values[1];

                        Console.WriteLine("pong event from {0}: seq={1}, time={2}/{3} s",
                            hostName,
                            values[2],
                            (dst_time - src_time).TotalSeconds,
                            (BroTime.Now - src_time).TotalSeconds);
                    });

                    connection.Connect();

                    Console.WriteLine("Bro connection established. Starting ping cycle, press any key to cancel...");

                    int seq = 0;

                    while (!Console.KeyAvailable)
                    {
                        // Send ping
                        connection.SendEvent("ping", BroTime.Now, new BroValue(seq++, BroType.Count));

                        // Process any received responses
                        connection.ProcessInput();

                        // Wait one second between pings
                        Thread.Sleep(1000);
                    }
                }

                return 0;
            }
            catch (Exception ex)
            {
                Console.Write("Exception: {0}{1}", ex.Message, Environment.NewLine);
                return 1;
            }
        }
    }
}
