using System;
using PostgreListen;
using System.Threading;

namespace PostgresListenTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            CancellationTokenSource source = new CancellationTokenSource();
            CancellationToken token = source.Token;
            
            var jono = PostgreListen.Listener.Listen(
                new ConnectionInformation {
                    ConnectionString = "Server=127.0.0.1;Database=test;User Id=test;Password=test",
                    TimeoutSeconds = 30
                },
                new ListenParameters {
                    Channel = "abc"
                },
                token
            );

            
            foreach(var msg in jono){
                Console.WriteLine(msg.ToString());
                if(msg.ToString()=="quit"){
                    Console.WriteLine("Cancelling...");
                    source.Cancel();
                }
                
            }

        }
    }
}
