using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HttpPanoVideoStreamingServer
{
    public class StreamingServer:IDisposable
    {
        private List<Socket> _Clients;
        private Thread _Thread;
        public Socket Server { get; set; }
        private bool _isRunning = false;
        public int Interval { get; set; }
        public IEnumerable<Image> ImagesSource { get; set; }
        public IEnumerable<Socket> Clients { get { return _Clients; } }
        public StreamingServer(IEnumerable<Image> imagesSource)
        {
            _Thread = null;
            _Clients = new List<Socket>();
            this.ImagesSource = imagesSource;
            this.Interval = 80;
            _isRunning = false;
        }
        public bool IsRunning { get { return _isRunning; } }
        public void Start(int port)
        {
            lock (this)
            {
                _Thread = new Thread(new ParameterizedThreadStart(ServerThread));
                _Thread.IsBackground = true;
                _Thread.Start(port);
                _isRunning = true;
            }
        }
        public void Start()
        {
            this.Start(8080);
        }
        public void Stop()
        {

            if (this.IsRunning)
            {
                try
                {
                    //_Thread.Join();
                    //_Thread.Abort();
                    _Thread.Interrupt();
                    //_Thread.Join();

                }
                catch(Exception ex)
                {
                    //System.Diagnostics.Debug.WriteLine(ex.Message);
                }
                finally
                {

                    lock (_Clients)
                    {

                        foreach (var s in _Clients)
                        {
                            try
                            {
                                s.Close();
                            }
                            catch { }
                        }
                        _Clients.Clear();

                    }

                    _Thread = null;
                    _isRunning = false;
                    this.Server.Close();
                    this.Server = null;
                }
            }
        }
        private void ServerThread(object state)
        {

            try
            {
                this.Server= new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                Server.Bind(new IPEndPoint(IPAddress.Any, (int)state));
                Server.Listen(10);

                System.Diagnostics.Debug.WriteLine(string.Format("Server started on port {0}.", state));

                foreach (Socket client in getCommingConnections())
                    ThreadPool.QueueUserWorkItem(new WaitCallback(ClientThread), client);

            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }

            this.Stop();
        }
        IEnumerable<Socket> getCommingConnections()
        {
            while (this.IsRunning)
            {

                yield return this.Server?.Accept();
            }
            //server.Server?.Close();
            yield break;
        }
        private void ClientThread(object client)
        {

            Socket socket = (Socket)client;

            System.Diagnostics.Debug.WriteLine(string.Format("New client from {0}", socket.RemoteEndPoint.ToString()));

            lock (_Clients)
                _Clients.Add(socket);

            try
            {
                using (MjpegWriter wr = new MjpegWriter(new NetworkStream(socket, true)))
                {

                    // Writes the response header to the client.
                    wr.WriteHeader();

                    // Streams the images from the source to the client.
                    foreach (Image img in this.ImagesSource)
                    {
                        if (this.Interval > 0)
                            Thread.Sleep(this.Interval);

                        wr.Write(img);
                    }

                }
            }
            catch { }
            finally
            {
                lock (_Clients)
                    _Clients.Remove(socket);
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            this.Stop();
        }

        #endregion
    }
    static class SocketExtensions
    {

        public static IEnumerable<Socket> IncommingConnections(this StreamingServer server)
        {
            while (server.IsRunning)
            {
                
                yield return server.Server?.Accept();
            }
            //server.Server?.Close();
            yield break;
        }

    }
}
