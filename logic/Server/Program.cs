﻿using System;
using System.Threading;
using CommandLine;

namespace Server
{
    class Program
    {
        static int Main(string[] args)
        {
            foreach (var arg in args)
            {
                Console.Write($"{arg} ");
            }
            Console.WriteLine();

            ArgumentOptions? options = null;
            _ = Parser.Default.ParseArguments<ArgumentOptions>(args).WithParsed(o => { options = o; });
            if (options == null)
            {
                Console.WriteLine("Argument parsing failed!");
                return 1;
            }

            Console.WriteLine("Server begins to run: " + options.ServerPort.ToString());

            ServerBase? server = null;

            if (options.Playback)
            {
                try
                {
                    server = new PlayBackServer(options);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return 1;
                }
            }
            else
            {
                try
                {
                    server = new GameServer(options);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return 1;
                }
            }

            server.WaitForGame();

            Thread.Sleep(50);
            Console.WriteLine("");
            Console.WriteLine("===================  Final Score   ====================");
            for (int i = 0; i < server.TeamCount; ++i)
            {
                Console.WriteLine($"Team {i}: {server.GetTeamScore(i)}");
            }

            //if (server.ForManualOperation)
            //{
            //	Console.WriteLine("Press any key to continue...");
            //	Console.ReadKey();
            //}

            return 0;
        }
    }
}
