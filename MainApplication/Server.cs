using Ultrabox.ChromaSync;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ultrabox.ChromaSync
{
    class Server
    {
        public static Thread _clientThread;

        internal static void RunServer()
        {
            TcpListener server = null;
            try
            {
                // Set the TcpListener on port 13000.
                const int port = 13000;

                // TcpListener server = new TcpListener(port);
                server = new TcpListener(IPAddress.Any, port);
                //BalloonTip("Server running", IPAddress.Any.ToString());
                // Start listening for client requests.
                server.Start();

                while (true)
                {
                    if (server.Pending())
                    {
                        _clientThread = new Thread(() =>
                        {
                            using (TcpClient client = server.AcceptTcpClient())
                            {
                                ParseData(client);
                            }
                        });
                        _clientThread.Start();
                    }
                    Thread.Sleep(10);
                }

            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
        }

        private static void ParseData(TcpClient listener)
        {

            // Buffer for reading data
            Byte[] bytes = new Byte[256];

            try
            {
                // Enter the listening loop. 
                Debug.Write("Got a connection... ");

                // Perform a blocking call to accept requests. 
                // You could also user server.AcceptSocket() here.
                Debug.WriteLine("Connected!");


                // Get a stream object for reading and writing
                NetworkStream stream = listener.GetStream();


                byte[] myReadBuffer = new byte[1024];
                StringBuilder myCompleteMessage = new StringBuilder();
                int numberOfBytesRead = 0;

                // Incoming message may be larger than the buffer size. 
                do
                {
                    numberOfBytesRead = stream.Read(myReadBuffer, 0, myReadBuffer.Length);

                    myCompleteMessage.AppendFormat("{0}", Encoding.ASCII.GetString(myReadBuffer, 0, numberOfBytesRead));

                }
                while (stream.DataAvailable);
                if (myCompleteMessage.Length != 0)
                {

                    // Print out the received message to the console.
                    try
                    {
                        var split = myCompleteMessage.ToString().Split(new char[] { '{' }, 2);

                        if (split.Length > 1)
                        {

                            String ns = "{" + myCompleteMessage.ToString().Split(new char[] { '{' }, 2)[1];
                            myCompleteMessage = null;
                            //Debug.Write(myCompleteMessage);
                            string header = string.Format("HTTP/1.1 {0}\r\n"
                                              + "Server: {1}\r\n"
                                              + "Content-Type: {2}\r\n"
                                              + "Keep-Alive: Close\r\n"
                                              + "\r\n",
                                              "HTTP 200 OK", "Chroma Sync", "application/json");
                            // Send header & data
                            var headerBytes = Encoding.ASCII.GetBytes(header);
                            stream.Write(headerBytes, 0, headerBytes.Length);

                            try
                            {
                                JObject o = JObject.Parse(ns);
                                LuaScripting.PassThrough(o);
                            }
                            catch (Exception e)
                            {
                                Debug.WriteLine(e);
                            }
                            return;
                        }

                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e);
                    }
                }
            }
            catch { }
            finally
            {
                try
                {
                    listener.Close();
                }
                catch (Exception) { }
            }
        }


    }
}
