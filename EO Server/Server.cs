using System;
using System.Threading;

namespace EO_Server
{

    public class Server
    {
        public static Listener listener;
        public static int port = 11000;
        public static long MS_PER_UPDATE = 16;
        public static bool RUNNING = true;

        public static long GetCurrentTime()
        {
            return (DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond);
        }

        public static void OnProgExit(object? sender, ConsoleCancelEventArgs e)
        {
            e.Cancel = true;
            RUNNING = false;
        }

        static void Main(string[] args)
        {
            listener = new Listener(port);

            //Listen to the user's request to cancel execution of the server
            Console.CancelKeyPress += OnProgExit;

            Console.WriteLine("Starting NEO Server");
            DataFiles.LoadDataFiles();

            Console.WriteLine($"Loading in maps from {FileMap.mapFolderPath}");
            MapManager.CreateMaps();
            
            Console.WriteLine("Connecting to database");
            DB.Connect("myDB");
            
            while(!DB.Connected)
            {
                Thread.Sleep(1000);
                Console.WriteLine("Connecting to database");
                DB.Connect("myDB");
            }

            Console.WriteLine("Successfully connected to database");
           

            try
            {
                listener.StartListening();

                long previous = GetCurrentTime();
                long lag = 0;

                while (RUNNING)
                {
                    long current = GetCurrentTime();
                    long elapsed = current - previous;
                    previous = current;
                    lag += elapsed;

                    if (lag >= MS_PER_UPDATE)
                    {
                        listener.Update();
                        MapManager.Update();
                        
                        lag = 0;
                    }
                    

                    //Slow down, horsey!! Don't kill my CPU!
                    //Thread.Sleep(10);
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.StackTrace);
            }

            Console.WriteLine("Ending NEO Server");

            listener.Disconnect();
            listener.OnProgExit();

            Console.WriteLine("NEO Server closed");
        }


    }
}
