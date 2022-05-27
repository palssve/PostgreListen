using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Threading;
using Npgsql;


namespace PostgreListen
{

     public class ThreadParameters {
        public ConnectionInformation conn= null;
        public ListenParameters listen = null;
        public CancellationToken cancelToken = new CancellationToken();
        public ListenerEnumerator listener = null;
    } 

    public class ListenerEnumerator : IEnumerator {
        

        public ConcurrentQueue<string> payloads = new ConcurrentQueue<string>();
        public bool HasNext = true;
        public static Semaphore greenlight = new Semaphore(0,1);
        public static Semaphore bluelight = new Semaphore(0,1);
        public static CancellationToken cancelToken = new CancellationToken();
        // ManualResetEventSlim greenlight = new ManualResetEventSlim(); 


        


        public void handler(string new_payload){
            payloads.Enqueue(new_payload);
            bluelight.WaitOne();
            greenlight.Release();
        }

        public void stop(){
            HasNext = false;
            bluelight.WaitOne();
            greenlight.Release();
        }

        public ListenerEnumerator(){
            bluelight.Release();
        }

        



        public bool MoveNext() {
            // sync with the listen call here
            greenlight.WaitOne();
            
            return HasNext; // 
        }

        public void Reset(){
            //payloads.Clear(); // doesn't make sense
            
        } 

        object IEnumerator.Current
        {
            get
            {
                return Current;
                
            }
        }
        public string Current 
        {
            get {
                string output;
                if(payloads.TryDequeue(out output)){
                    bluelight.Release();
                    return output;
                }
                bluelight.Release();
                return null;
            }
        }

    }


    public class BoilerPlate : IEnumerable
    {
        
        public ListenerEnumerator myEnumerator;
        public BoilerPlate(ListenerEnumerator le)
        {
            myEnumerator = le;
        }

    // Implementation for the GetEnumerator method.
        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator) GetEnumerator();
        }

        public ListenerEnumerator GetEnumerator()
        {
            return myEnumerator;
        }
    }

    public class Listener
    {

        private static void threadProc(Object _params){
            var pars = (ThreadParameters) _params;
            using (var conn = new NpgsqlConnection(pars.conn.ConnectionString))
            {
                conn.Open();
                conn.Notification += (o,e) => {
                    pars.listener.handler(e.AdditionalInformation);
                };
                using (var cmd = new NpgsqlCommand($"Listen {pars.listen.Channel}",conn))
                {
                    cmd.ExecuteNonQuery();
                }
                var stopped = false;
                while(!pars.cancelToken.IsCancellationRequested && !stopped){
                    var wt = conn.WaitAsync(pars.cancelToken);
                    try {
                        wt.Wait();
                    } catch (System.AggregateException)
                    {
                        stopped = true;
                    }

                }
                
                pars.listener.stop();
            }
        }
        /// <summary>
        /// Query data using PostgreSQL. Documentation: https://github.com/CommunityHiQ/Frends.Community.Postgre
        /// </summary>
        /// <param name="queryParameters"></param>
        /// <param name="connectionInfo"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>Object {string Result}</returns>
        public static BoilerPlate Listen(ConnectionInformation connectionInfo, ListenParameters listenParameters, CancellationToken cancellationToken)
        {

            ListenerEnumerator listen = new ListenerEnumerator();
            BoilerPlate useless = new BoilerPlate(listen);
            var thparms = new ThreadParameters {
                conn = connectionInfo,
                listen = listenParameters,
                cancelToken = cancellationToken,
                listener = listen
            };

            var th = new Thread(threadProc);
            th.Start(thparms);
            return useless;
        }
    }
}
