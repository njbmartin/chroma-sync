using System;
using System.Text;
using System.Windows.Forms;

using System.Net;
using System.Net.Sockets;

using Newtonsoft.Json.Linq;
using Corale.Colore.Core;
using Corale.Colore.Razer;
using System.Threading;
using System.Diagnostics;
using Chroma_Sync.Properties;
using System.Reflection;
using System.IO;
using Corale.Colore;
using Corale.Colore.Razer.Mouse;

namespace Chroma_Sync
{


    public class TrayApplicationContext : ApplicationContext
    {
        private readonly NotifyIcon _icon;
        private readonly Thread _serverThread;

        private Thread _deadThread;
        private Thread _flashThread;


        private String _team;

        private bool _isDead;
        private bool _isFlashed;
        private int _roundKills;

        //Program configWindow = new Program();
        public TrayApplicationContext()
        {
            const string configMenuText = "Configuration";
            _isFlashed = true;
            _isDead = true;

            MenuItem configMenuItem = new MenuItem(configMenuText, ShowConfig);
            MenuItem exitMenuItem = new MenuItem("Exit", Exit);

            _icon = new NotifyIcon
            {
                Icon = Properties.Resources.favicon,
                ContextMenu = new ContextMenu(new[] { exitMenuItem }),
                Visible = true,
                Text = Resources.ExitMenuText,
            };
            BalloonTip("Getting things ready", "Chroma Sync is performing first-time setup.\nThis shouldn't take long...");
            Debug.WriteLine(Chroma.Instance.Query(Devices.MambaTeChroma).Connected ? "connected" : "not connected");

            var folder = GameLocator.InstallFolder("Counter-Strike Global Offensive");
            if (folder != null)
            {
                var file = Resources.gamestate_integration_chromasync;
                Stream stream = new MemoryStream(file);
                CopyResource(stream, folder + "\\csgo\\cfg\\gamestate_integration_chromasync.cfg");
                Debug.WriteLine(folder);
                BalloonTip("Chroma Sync", "Chroma Sync is now running in the background.");
            }
            else
            {
                BalloonTip("Chroma Sync", "CS:GO folder was not found");
                Debug.WriteLine("CS:GO folder was not found");
            }
            //Mouse.Instance.SetLed(Led.Strip10, Color.Red);
            var volumeThread = new Thread(CheckVolume);
            volumeThread.Start();
            //Flashed();
            _serverThread = new Thread(RunServer);
            _serverThread.Start();
        }

        private void CheckVolume()
        {
            while (true)
            {
                var cv = AudioVolume.Volume;
                var volume = AudioVolume.GetMasterVolume();
                if (Math.Abs(cv - volume) > 0.01f)
                {
                    ResetAll();
                    BalloonTip("Sound Volume", "Currently set to " + (volume * 100) + "%");
                    var total = 14 * volume;
                    
                    for (uint i = 1; i <= 14; i++)
                    {
                        Debug.WriteLine((Led)(i + 3));
                        Mouse.Instance.SetLed((Led) (i + 3), i < total ? Color.Blue : Color.Black);
                    }
                }
                Thread.Sleep(1000);
            }
        }

        private void CopyResource(Stream stream, string file)
        {
            using (stream)
            {
                if (stream == null)
                {
                    throw new ArgumentException("No such resource", "resourceName");
                }
                using (Stream output = File.OpenWrite(file))
                {
                    stream.CopyTo(output);
                }
            }
        }

        public void BalloonTip(string title, string text)
        {
            _icon.BalloonTipTitle = title;
            _icon.BalloonTipText = text;
            _icon.ShowBalloonTip(2000);
        }

        public void ResetAll()
        {
            Mouse.Instance.SetEffect(Corale.Colore.Razer.Mouse.Effects.Effect.None);
            Headset.Instance.SetEffect(Corale.Colore.Razer.Headset.Effects.Effect.None);
            Keyboard.Instance.SetEffect(Corale.Colore.Razer.Keyboard.Effects.Effect.None);
            Keypad.Instance.SetEffect(Corale.Colore.Razer.Keypad.Effects.Effect.None);
            Mousepad.Instance.SetEffect(Corale.Colore.Razer.Mousepad.Effects.Effect.None);
            Thread.Sleep(10);
        }

        public void SetToTeamColour()
        {
            ResetAll();
            Console.WriteLine("Team: " + _team);
            if (_team == "T")
            {
                SetAll(new Color(255, 69, 0));
            }
            else if (_team == "CT")
            {
                SetAll(new Color(0, 191, 255));
            }
            else
            {
                SetAll(Color.White);
            }
        }

        void RoundKills(int kills)
        {
            if (_roundKills == kills)
                return;
            for (uint i = 1; i <= 12; i++)
            {
                Keyboard.Instance.SetPosition(0, 3 + i, kills >= i ? Color.HotPink : Color.Black);
            }
            _roundKills = kills;
        }


        Color RGBAFix(Color color)
        {
            var a = color.A;

            return new Color(
                (1 - a) * 0 - a * color.R,
                (1 - a) * 0 - a * color.G,
                (1 - a) * 0 - a * color.B
            );
        }

        public void SetAll(Color color)
        {
            Headset.Instance.SetAll(color);
            Keyboard.Instance.SetAll(color);
            Keypad.Instance.SetAll(color);
            Mouse.Instance.SetAll(color);
            Mousepad.Instance.SetAll(color);
            Thread.Sleep(20);
        }

        public void Flashed()
        {
            BalloonTip("Flashed", "Flash animation should be shown");
            ResetAll();
            SetAll(Color.White);
            Thread.Sleep(1000);
            for (var i = 0f; i <= 1f; i = i + 0.001f)
            {
                Debug.WriteLine(i);
                var brightness = new Color(1f, 1f, 1f, i);
                SetAll(RGBAFix(brightness));
            }
            ResetAll();
            SetToTeamColour();
        }

        public void Died()
        {
            BalloonTip("Dead", "You died. Oh no. What a shame.");
            for (var i = 0; i <= 6; i++) // flash 6 times
            {
                ResetAll();
                Thread.Sleep(100);
                SetAll(Color.Red);
                Thread.Sleep(100);
            }
            ResetAll();
            SetToTeamColour();
        }

        void WeaponsAmmo(JObject weapons)
        {
            Debug.WriteLine("Checking ammo...");

            var oneSet = false;
            var twoSet = false;
            var threeSet = false;
            foreach (var weapon in weapons)
            {
                JObject wObj = weapon.Value.ToObject<JObject>();
                var ammoMax = wObj.Value<float>("ammo_clip_max");
                var ammoCurrent = wObj.Value<float>("ammo_clip");
                Color color = Color.Black;
                Corale.Colore.Razer.Keyboard.Key key = Corale.Colore.Razer.Keyboard.Key.Invalid;
                if (Math.Abs(ammoCurrent) > 0.1f)
                {
                    var percentage = (ammoCurrent / ammoMax * 100);

                    if (percentage > 50)
                    {
                        color = Color.Green;
                    }
                    else if (percentage > 25)
                    {
                        color = Color.Yellow;
                    }
                    else
                    {
                        color = Color.Red;
                    }



                }



                switch (wObj.Value<string>("type"))
                {
                    case "Knife":

                        key = Corale.Colore.Razer.Keyboard.Key.Three;
                        threeSet = true;
                        break;
                    case "Pistol":
                        key = Corale.Colore.Razer.Keyboard.Key.Two;
                        twoSet = true;
                        break;

                    case "C4":
                        key = Corale.Colore.Razer.Keyboard.Key.Zero;
                        break;
                    default:
                        key = Corale.Colore.Razer.Keyboard.Key.One;
                        oneSet = true;
                        break;
                }
                Keyboard.Instance.SetKey(key, color, false);
            }

            if (!oneSet)
            {
                Keyboard.Instance.SetKey(Corale.Colore.Razer.Keyboard.Key.One, Color.Black, false);
            }

            if (!twoSet)
            {
                Keyboard.Instance.SetKey(Corale.Colore.Razer.Keyboard.Key.Two, Color.Black, false);
            }

            if (!threeSet)
            {
                Keyboard.Instance.SetKey(Corale.Colore.Razer.Keyboard.Key.Three, Color.Black, false);
            }


        }


        void ShowConfig(object sender, EventArgs e)
        {
            /*
            // If we are already showing the window, merely focus it.
            if (configWindow.Visible)
            {
                configWindow.Activate();
            }
            else
           { 
                configWindow.ShowDialog();
            }
            */
        }

        void Exit(object sender, EventArgs e)
        {
            // We must manually tidy up and remove the icon before we exit.
            // Otherwise it will be left behind until the user mouses over.
            _icon.Visible = false;
            _serverThread.Abort();
            Application.Exit();
        }





        void RunServer()
        {
            TcpListener server = null;
            try
            {
                // Set the TcpListener on port 13000.
                const int port = 13000;

                // TcpListener server = new TcpListener(port);
                server = new TcpListener(IPAddress.Any, port);

                // Start listening for client requests.
                server.Start();

                // Buffer for reading data
                Byte[] bytes = new Byte[256];


                // Enter the listening loop. 
                while (true)
                {
                    Debug.Write("Waiting for a connection... ");

                    // Perform a blocking call to accept requests. 
                    // You could also user server.AcceptSocket() here.
                    TcpClient client = server.AcceptTcpClient();
                    Debug.WriteLine("Connected!");


                    // Get a stream object for reading and writing
                    NetworkStream stream = client.GetStream();


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
                                Debug.Write("You received the following message : " + ns);
                                //Debug.Write(myCompleteMessage);
                                string header = string.Format("HTTP/1.1 {0}\r\n"
                                                  + "Server: {1}\r\n"
                                                  + "Content-Length: {2}\r\n"
                                                  + "Content-Type: {3}\r\n"
                                                  + "Keep-Alive: Close\r\n"
                                                  + "\r\n",
                                                  "HTTP 200 OK", "", 0, "application/json");
                                // Send header & data
                                var headerBytes = Encoding.ASCII.GetBytes(header);
                                stream.Write(headerBytes, 0, headerBytes.Length);



                                JObject o = JObject.Parse(ns);

                                var provider = o["provider"];


                                var player = o["player"];
                                if (player != null && provider["steamid"].ToObject<String>() == player["steamid"].ToObject<String>())
                                {
                                    // Do things as the player

                                    String nt = null;
                                    if (player["team"] != null)
                                    {
                                        nt = player["team"].ToObject<String>();
                                    }

                                    String activity = player["activity"].ToObject<String>();
                                    if (_team == null || _team == "NA" || nt != _team)
                                    {
                                        _team = "NA";
                                        if (player["team"] != null)
                                        {
                                            _team = player["team"].ToObject<String>();
                                        }
                                        SetToTeamColour();
                                    }
                                    var state = player["state"];
                                    if (state != null && activity != "menu" && player["team"] != null)
                                    {
                                        if (state["flashed"] != null && state["flashed"].ToObject<int>() > 0)
                                        {
                                            if (_flashThread == null || !_flashThread.IsAlive)
                                            {
                                                if (!_isFlashed)
                                                {
                                                    _isFlashed = true;
                                                    _flashThread = new Thread(new ThreadStart(Flashed));
                                                    _flashThread.Start();
                                                }
                                            }
                                        }
                                        else
                                        {
                                            _isFlashed = false;
                                        }


                                        if (state["health"] != null)
                                        {

                                            if (state["health"].ToObject<int>() == 0)
                                            {
                                                if (!_isDead)
                                                {
                                                    _isDead = true;
                                                    _deadThread = new Thread(new ThreadStart(Died));
                                                    _deadThread.Start();
                                                }
                                            }
                                            else
                                            {
                                                _isDead = false;

                                            }
                                        }
                                        RoundKills(state["round_kills"].ToObject<int>());

                                        if (player["weapons"] != null)
                                        {
                                            var weapons = player["weapons"].ToObject<JObject>();
                                            WeaponsAmmo(weapons);
                                        }
                                    }


                                }

                            }
                        }
                        catch (Exception e)
                        {
                            Debug.WriteLine(e);
                        }
                    }
                    // Shutdown and end connection
                    client.Close();
                }
            }
            catch (SocketException e)
            {
                Debug.WriteLine("SocketException: {0}", e);
            }
            finally
            {
                // Stop listening for new clients.
                server.Stop();
            }


            Debug.WriteLine("\nHit enter to continue...");
        }


    }
}
