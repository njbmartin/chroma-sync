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
using ChromaSync.Properties;
using System.IO;
using Neo.IronLua;

namespace ChromaSync
{


    public class TrayApplicationContext : ApplicationContext
    {
        private readonly NotifyIcon _icon;

        private Thread _serverThread;
        private Thread _deadThread;
        private Thread _flashThread;
        private Thread _volumeThread;
        private Thread _gtaThread;

        private String _team;

        private bool _isDead;
        private bool _isPlanted;
        private bool _isFreezeTime;
        private bool _isAnimating;
        private bool _isFlashed;

        private int _roundKills;

        public static Lua l = new Lua();
        private Form1 _mainWindow;
        private Thread _clientThread;
        private Thread _luaThread;

        //Program configWindow = new Program();
        public TrayApplicationContext()
        {

            _isFlashed = true;
            _isDead = true;
            _mainWindow = new Form1();
            if (!Settings.Default.FirstRun)
            {
                Settings.Default.FirstRun = true;
                _mainWindow.Show();
                Settings.Default.Save();
            }
            MenuItem about = new MenuItem("About Chroma Sync", showAbout);
            MenuItem updates = new MenuItem("Check for updates...", ShowConfig);
            MenuItem exitMenuItem = new MenuItem("Exit", Exit);
            var cm = new ContextMenu();
            cm.MenuItems.Add(about);
            cm.MenuItems.Add(updates);
            cm.MenuItems.Add(exitMenuItem);
            EventHook.MouseHook.Start();
            _icon = new NotifyIcon
            {
                Icon = Properties.Resources.favicon,
                ContextMenu = cm,
                Visible = true,
                Text = Resources.ExitMenuText,
            };
            _icon.DoubleClick += new System.EventHandler(ShowConfig);
            //BalloonTip("Getting things ready", "Chroma Sync is performing first-time setup.\nThis shouldn't take long...");
            Debug.WriteLine(Chroma.Instance.Query(Devices.MambaTeChroma).Connected ? "connected" : "not connected");
            string folder = null;

            folder = GameLocator.InstallFolder("Counter-Strike Global Offensive");
            if (folder != null)
            {
                var file = Resources.gamestate_integration_chromasync;
                Stream stream = new MemoryStream(file);
                CopyResource(stream, folder + "\\csgo\\cfg\\gamestate_integration_chromasync.cfg");
                Debug.WriteLine(folder);
                

            }
            else
            {
                //BalloonTip("CS:GO Configuration", "CS:GO folder was not found on this computer");
                Debug.WriteLine("CS:GO folder was not found");
            }



            folder = GameLocator.InstallFolder("Grand Theft Auto V");
            if (folder != null)
            {
                Debug.WriteLine(folder);
                _gtaThread = new Thread(GTA.Task);
                //_gtaThread.Start(folder);
            }
            else
            {
                //BalloonTip("GTA V Configuration", "GTA V directory not found.");
                Debug.WriteLine("GTA V directory not found");
            }



            _serverThread = new Thread(RunServer);
            _serverThread.Start();

            _volumeThread = new Thread(CheckVolume);
            //_volumeThread.Start();

            _luaThread = new Thread(LuaScripting.LuaThread);
            _luaThread.Start();

        }

        void showAbout(object sender, EventArgs e)
        {
            Process.Start("http://chromasync.io/about/");
        }


        public static class GTA
        {
            private static double _currentAmmo, _wantedLevel;

            public static void Task(object folder)
            {
                string f = (string)folder;
                Debug.WriteLine(f);
                FileSystemWatcher watcher = new FileSystemWatcher();

                watcher.Path = f;
                watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite
               | NotifyFilters.FileName | NotifyFilters.DirectoryName;
                // Only watch text files.
                watcher.Filter = "chroma_sync_*";

                watcher.Changed += new FileSystemEventHandler(OnChanged);
                watcher.Created += new FileSystemEventHandler(OnChanged);
                watcher.EnableRaisingEvents = true;
            }




            private static void OnChanged(object source, FileSystemEventArgs e)
            {
                // Specify what is done when a file is changed, created, or deleted.
                string readText = "";
                try
                {
                    readText = File.ReadAllText(e.FullPath);
                }
                catch (Exception error)
                {
                    return;
                }

                if (e.Name.Contains("ammo"))
                {
                    int ammo = int.Parse(readText);

                    UpdateAmmo(ammo);
                }

                if (e.Name.Contains("police"))
                {
                    _wantedLevel = double.Parse(readText);
                    Debug.WriteLine(_wantedLevel);
                }
            }

            public static void UpdateAmmo(int ammo)
            {
                var keys = Math.Round((double)ammo / 100 * 12);
                if (keys != _currentAmmo)
                {
                    for (uint i = 1; i <= 12; i++)
                    {
                        try
                        {
                            Keyboard.Instance.SetPosition(0, 2 + i, keys >= i ? Color.Red : Color.Black);
                        }
                        catch (Exception e) { }
                    }
                }
                _currentAmmo = keys;
            }


            public static void PoliceAnimation()
            {
                int currentLevel = 0;
                while (true)
                {
                    if (currentLevel != _wantedLevel)
                    {
                        double wLevel = _wantedLevel;
                        try
                        {
                            Keyboard.Instance.SetKey(Corale.Colore.Razer.Keyboard.Key.Macro1, wLevel > 0 ? Color.Red : Color.Black);
                            Keyboard.Instance.SetKey(Corale.Colore.Razer.Keyboard.Key.Macro2, wLevel > 1 ? Color.Red : Color.Black);
                            Keyboard.Instance.SetKey(Corale.Colore.Razer.Keyboard.Key.Macro3, wLevel > 2 ? Color.Red : Color.Black);
                            Keyboard.Instance.SetKey(Corale.Colore.Razer.Keyboard.Key.Macro4, wLevel > 3 ? Color.Red : Color.Black);
                            Keyboard.Instance.SetKey(Corale.Colore.Razer.Keyboard.Key.Macro5, wLevel > 4 ? Color.Red : Color.Black);
                            Thread.Sleep(200);
                            Keyboard.Instance.SetKey(Corale.Colore.Razer.Keyboard.Key.Macro1, wLevel > 0 ? Color.Blue : Color.Black);
                            Keyboard.Instance.SetKey(Corale.Colore.Razer.Keyboard.Key.Macro2, wLevel > 1 ? Color.Blue : Color.Black);
                            Keyboard.Instance.SetKey(Corale.Colore.Razer.Keyboard.Key.Macro3, wLevel > 2 ? Color.Blue : Color.Black);
                            Keyboard.Instance.SetKey(Corale.Colore.Razer.Keyboard.Key.Macro4, wLevel > 3 ? Color.Blue : Color.Black);
                            Keyboard.Instance.SetKey(Corale.Colore.Razer.Keyboard.Key.Macro5, wLevel > 4 ? Color.Blue : Color.Black);
                        }
                        catch (Exception e) { }
                    }
                    Thread.Sleep(200);
                }
            }
        }

        private void CheckVolume()
        {
            while (true)
            {
                var cv = AudioVolume.Volume;
                var volume = AudioVolume.GetMasterVolume();
                if (Math.Abs(cv - volume) > 0.01f)
                {
                    //ResetAll();
                    //BalloonTip("Sound Volume", "Currently set to " + (volume * 100) + "%");

                    var headsetTotal = Math.Round(10 * volume);
                    var c = Color.Green;
                    if (headsetTotal >= 4)
                        c = new Color(255, 140, 0);
                    if (headsetTotal >= 5)
                        c = Color.Red;
                    Headset.Instance.SetAll(c);


                    var mouseTotal = 6 * volume;
                    var mousepadTotal = 14 * volume;
                    var custom = new Corale.Colore.Razer.Mousepad.Effects.Custom(new Color());
                    for (uint i = 0; i < Corale.Colore.Razer.Mousepad.Constants.MaxLeds; i++)
                    {
                        c = Color.Green;
                        if (i >= 7)
                            c = Color.Orange;
                        if (i >= 10)
                            c = Color.Red;
                        custom.Colors[i] = (i < mousepadTotal ? c : Color.Black);
                    }
                    Mousepad.Instance.SetCustom(custom);


                    var mouseCustom = new Corale.Colore.Razer.Mouse.Effects.Custom(new Color());
                    for (uint i = 0; i <= 7; i++)
                    {
                        c = Color.Green;
                        if (i >= 2)
                            c = Color.Orange;
                        if (i >= 4)
                            c = Color.Red;
                        mouseCustom.Colors[17 - i] = (i < mouseTotal ? c : Color.Black);
                        mouseCustom.Colors[9 - i] = (i < mouseTotal ? c : Color.Black);
                    }
                    Mouse.Instance.SetCustom(mouseCustom);
                }
                //Thread.Sleep(100);
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
            try
            {
                Mouse.Instance.SetEffect(Corale.Colore.Razer.Mouse.Effects.Effect.None);
                Headset.Instance.SetEffect(Corale.Colore.Razer.Headset.Effects.Effect.None);
                Keyboard.Instance.SetEffect(Corale.Colore.Razer.Keyboard.Effects.Effect.None);
                Keypad.Instance.SetEffect(Corale.Colore.Razer.Keypad.Effects.Effect.None);
                Mousepad.Instance.SetEffect(Corale.Colore.Razer.Mousepad.Effects.Effect.None);
            }
            catch (Exception e) { }
            Thread.Sleep(2);
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
                Keyboard.Instance.SetPosition(0, 2 + i, kills >= i ? Color.HotPink : Color.Black);
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
            Thread.Sleep(5);
        }

        public void Flashed()
        {
            _isAnimating = true;
            //BalloonTip("Flashed", "Flash animation should be shown");
            ResetAll();
            for (var i = 0f; i <= 1f; i = i + 0.005f)
            {
                Debug.WriteLine(i);
                var brightness = new Color(1f, 1f, 1f, i);
                SetAll(RGBAFix(brightness));
            }
            ResetAll();
            _isAnimating = false;
            SetToTeamColour();
        }

        public void Died()
        {
            _isAnimating = true;
            ResetAll();
            //BalloonTip("Dead", "You died. Oh no. What a shame.");
            for (var i = 0; i <= 6; i++) // flash 6 times
            {


                SetAll(Color.Red);
                Thread.Sleep(100);
                ResetAll();
                Thread.Sleep(100);
            }
            ResetAll();
            _isAnimating = false;
            SetToTeamColour();
            
        }

        public void Frozen()
        {
            _isAnimating = true;
            //BalloonTip("Dead", "You died. Oh no. What a shame.");
            while (_isFreezeTime)
            {

                ResetAll();
                Thread.Sleep(500);
                SetAll(Color.HotPink);
                Thread.Sleep(500);
            }
            ResetAll();
            _isAnimating = false;
            SetToTeamColour();
            
        }


        public void Planted()
        {
            _isAnimating = true;
            //BalloonTip("Dead", "You died. Oh no. What a shame.");
            while (_isPlanted)
            {

                ResetAll();
                Thread.Sleep(1000);
                SetAll(Color.Pink);
                Thread.Sleep(500);

            }
            ResetAll();
            _isAnimating = false;
            SetToTeamColour();
            
        }

        void WeaponsAmmo(JObject weapons)
        {
            if (_isAnimating)
                return;
            Debug.WriteLine("Checking ammo...");

            var oneSet = false;
            var twoSet = false;
            var threeSet = false;
            var fourSet = false;
            var fiveSet = false;
            foreach (var weapon in weapons)
            {
                JObject wObj = weapon.Value.ToObject<JObject>();
                var ammoMax = wObj.Value<float>("ammo_clip_max");
                var ammoCurrent = wObj.Value<float>("ammo_clip");
                Color color = Color.Green;
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
                        if(!threeSet)
                            key = Corale.Colore.Razer.Keyboard.Key.Three;
                        threeSet = true;
                        break;
                    case "Pistol":
                        if (!twoSet)
                            key = Corale.Colore.Razer.Keyboard.Key.Two;
                        twoSet = true;
                        break;

                    case "Grenade":
                        if (!fourSet)
                            key = Corale.Colore.Razer.Keyboard.Key.Four;
                        fourSet = true;
                        break;

                    case "C4":
                        if (!fiveSet)
                            key = Corale.Colore.Razer.Keyboard.Key.Five;
                        fiveSet = true;
                        break;
                    default:
                        if (!wObj.Value<string>("name").Contains("taser"))
                        {
                            if (!oneSet)
                                key = Corale.Colore.Razer.Keyboard.Key.One;
                            oneSet = true;
                        }
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

            if (!fourSet)
            {
                Keyboard.Instance.SetKey(Corale.Colore.Razer.Keyboard.Key.Four, Color.Black, false);
            }

            if (!fiveSet)
            {
                Keyboard.Instance.SetKey(Corale.Colore.Razer.Keyboard.Key.Five, Color.Black, false);
            }

        }


        void ShowConfig(object sender, EventArgs e)
        {
            if (_mainWindow.IsDisposed)
            {
                _mainWindow = new Form1();
            }
            // If we are already showing the window, merely focus it.
            if (_mainWindow.Visible)
            {
                _mainWindow.Activate();
            }
            else
            {
                _mainWindow.ShowDialog();
            }

        }

        void Exit(object sender, EventArgs e)
        {
            // We must manually tidy up and remove the icon before we exit.
            // Otherwise it will be left behind until the user mouses over.
            _icon.Visible = false;

            // Abort all threads
            if(_serverThread != null && _serverThread.IsAlive)
                _serverThread.Abort();
            if (_luaThread != null && _luaThread.IsAlive)
                _luaThread.Abort();
            //_gtaThread.Abort();
            if (_clientThread != null && _clientThread.IsAlive)
                _clientThread.Abort();

            Application.Exit();
        }





        private void RunServer()
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

        private void ParseData(TcpClient listener)
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
                            Debug.Write("You received the following message : " + ns);
                            //Debug.Write(myCompleteMessage);
                            string header = string.Format("HTTP/1.1 {0}\r\n"
                                              + "Server: {1}\r\n" 
                                              + "Content-Type: {3}\r\n"
                                              + "Keep-Alive: Close\r\n"
                                              + "\r\n",
                                              "HTTP 200 OK", "", 0, "application/json");
                            // Send header & data
                            var headerBytes = Encoding.ASCII.GetBytes(header);
                            stream.Write(headerBytes, 0, headerBytes.Length);

                            try {
                                JObject o = JObject.Parse(ns);
                                Debug.WriteLine(o.ToString());
                                LuaScripting.PassThrough(o);
                            }catch(Exception e)
                            {
                                Debug.WriteLine(e);
                            }
                            return;
                            /*var provider = o["provider"];
                            
                            var round = o["round"];

                            if (round != null)
                            {
                                var phase = round["phase"].ToObject<String>();

                                if (phase.Equals("freezetime"))
                                {
                                    _isFreezeTime = true;
                                    if (!_isAnimating)
                                    {
                                        new Thread(new ThreadStart(Frozen)).Start();
                                    }

                                }
                                else
                                {
                                    _isFreezeTime = false;
                                }


                                if (round["bomb"] != null && round["bomb"].ToObject<String>().Equals("planted"))
                                {
                                    _isPlanted = true;
                                    if (!_isAnimating)
                                    {
                                        new Thread(new ThreadStart(Planted)).Start();
                                    }

                                }
                                else
                                {
                                    _isPlanted = false;
                                }





                            }
                            else
                            {
                                _isFreezeTime = false;
                                _isPlanted = false;
                            }

                            var player = o["player"];
                            if (player != null && provider["steamid"].ToObject<String>() == player["steamid"].ToObject<String>())
                            {

                                String nt = null;
                                if (player["team"] != null)
                                {
                                    nt = player["team"].ToObject<String>();
                                }
                                else if (!_team.Equals("NA"))
                                {
                                    _team = "NA";
                                    SetToTeamColour();
                                }

                                String activity = player["activity"].ToObject<String>();
                                if (player["team"] != null && nt != _team)
                                {
                                    _team = player["team"].ToObject<String>();
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
                        */
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
