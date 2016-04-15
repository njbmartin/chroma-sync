using Corale.Colore.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Jint;
using Newtonsoft.Json.Linq;
using System.Diagnostics;

namespace Ultrabox.ChromaSync.Engines
{
    class JavascriptEngine : IDisposable
    {
        private readonly object _syncObject = new object();
        private readonly object debugLock = new object();
        private List<dynamic> callbacks;
        private Collection<Thread> scriptThreads;
        private FileSystemWatcher watcher;
        private bool _shouldStop;


        public void RequestStop()
        {
            CloseScripts();
            _shouldStop = true;
        }

        internal void CloseScripts()
        {
            callbacks = new List<dynamic>();
            try
            {
                foreach (var script in scriptThreads)
                    script.Abort();
            }
            catch (Exception e)
            {
                App.Log.Error(e);
            }
 
        }

        public void Start()
        {
            if (watcher == null)
                Watch();

            // WE NEED TO ENSURE CHROMA IS INITIALISED
            callbacks = new List<dynamic>();
            scriptThreads = new Collection<Thread>();
                    
            string path = @"%appdata%\ChromaSync";
            path = Environment.ExpandEnvironmentVariables(path);

            string scriptsPath = Path.Combine(path, "scripts");
            string packagesPath = Path.Combine(path, "packages");
            if (!Directory.Exists(scriptsPath))
                Directory.CreateDirectory(scriptsPath);


            // Todo: Get all scripts including the packages
            var files = Directory.GetFiles(path, "*.js", SearchOption.AllDirectories);

            foreach (string st in files)
            {
                var v = RegistryKeeper.GetValue(st);
                MenuItem menuItem = new MenuItem(Path.GetFileName(st));
                menuItem.Name = Path.GetFileName(st);
                menuItem.Tag = st;
                menuItem.Click += MenuItem_Click;
                if (!st.Contains("\\ChromaSync\\packages\\"))
                {
                    App.scriptsMenu.MenuItems.Add(menuItem);
                }
                if (v.Equals("True"))
                {
                    menuItem.Checked = true;
                    scriptThreads.Add(
                    new Thread(() =>
                    {
                                 
                        Engine jint = new Engine(cfg => cfg.AllowClr(typeof(Color).Assembly));
                        jint.SetValue("debug", new Func<object, bool>(debug));
                        //dg.ConvertInt = new Func<object, int>(convertInt);
                        //dg.GetType = new Func<object, object>(GetType);
                        //dg.NewCustom = new Func<string, Color, object>(newCustom);
                        //dg.IntToByte = new Func<int, byte>(IntToByte);
                        jint.SetValue("Headset", Headset.Instance);
                        jint.SetValue("Keyboard", Keyboard.Instance);
                        jint.SetValue("Keypad", Keypad.Instance);
                        jint.SetValue("Mouse", Mouse.Instance);
                        jint.SetValue("Chroma", Chroma.Instance);
                        jint.SetValue("Mousepad", Mousepad.Instance);
                        jint.SetValue("Color",  new Func<byte,byte,byte,Color>(JintColor));
                        jint.SetValue("RegisterForEvents", new Action<string, object>(registerEvents));
                        debug("starting Jint for: " + st);
                        try
                        {
                            string readText = File.ReadAllText(st);
                            jint.Execute(readText);
                        }
                        catch (Exception e)
                        {
                            Debug.WriteLine(e.Message);
                            //    App.Log.Error(e);
                            //Thread.ResetAbort();
                        }

                    }));
                    scriptThreads.Last().Start();
                }
            }
        }

        public Color JintColor(byte r, byte g, byte b)
        {
            return new Color(r, g, b);
        }


        private void MenuItem_Click(object sender, EventArgs e)
        {
            MenuItem s = (MenuItem)sender;
            RegistryKeeper.UpdateReg((string)s.Tag, (!s.Checked).ToString());
            App.RestartServices();
        }

        public static int convertInt(object o)
        {
            return Convert.ToInt32(o);
        }

        public static byte IntToByte(int o)
        {
            return (byte)o;
        }

        public static object GetType(object o)
        {
            debug(o.GetType());
            return o;
        }



        public static bool debug(object d)
        {
            var text = DateTime.Now + " - " + d;
            Debug.WriteLine(text);
            App.Log.Debug(text);
            return true;
        }


        public void PassThrough(JObject json)
        {
            JObject j = (JObject)json.DeepClone();
            
            var l=  new List<Jint.Native.JsValue>() { j.ToString() };
            foreach (JintCallback action in callbacks)
            {
                var name = json["provider"] != null ? json["provider"]["name"].ToString() : json["product"]["name"].ToString();
                if (action.name == name)
                {
                    try
                    {
                        //action.callback(l);
                        
                        debug("Data passed to " + action.name);
                    }
                    catch (Exception e)
                    {
                        App.Log.Error(e);
                    }
                }

            }
        }

        public static object newCustom(string t, Color c)
        {
            switch (t)
            {
                case "mouse":
                    return new Corale.Colore.Razer.Mouse.Effects.Custom(c);
                case "mousepad":
                    return new Corale.Colore.Razer.Mousepad.Effects.Custom(c);
                case "keypad":
                    return new Corale.Colore.Razer.Keypad.Effects.Custom(c);
                case "keyboard":
                    return new Corale.Colore.Razer.Keyboard.Effects.Custom(c);
                default:
                    return null;
            }
        }

        public void registerEvents(string n, object c)
        {

            callbacks.Add(new JintCallback { name = n, callback = (Func<string,object,object>)c });
            debug("Registered Callback: " + n);
        }


        private void OnChanged(object source, FileSystemEventArgs e)
        {
            Debug.WriteLine("Changed");
            watcher.EnableRaisingEvents = false;
            App.RestartServices();
        }


        private void Watch()
        {
            watcher = new FileSystemWatcher();
            string path = @"%appdata%\ChromaSync";
            path = Environment.ExpandEnvironmentVariables(path);
            var sp = Path.Combine(path, "scripts");
            if (!Directory.Exists(sp))
                Directory.CreateDirectory(sp);


            watcher.Path = sp;
            watcher.NotifyFilter = NotifyFilters.LastWrite
           | NotifyFilters.FileName | NotifyFilters.DirectoryName;
            watcher.Filter = "*.js";
            Debug.WriteLine("Enabled watch");
            watcher.Created += new FileSystemEventHandler(OnChanged);
            watcher.Changed += new FileSystemEventHandler(OnChanged);
            watcher.EnableRaisingEvents = true;
        }

        public void Dispose()
        {
            watcher.Dispose();
            throw new NotImplementedException();
        }

        public class JintCallback
        {
            public string name { get; set; }
            public Func<string,object,object> callback { get; set; }
        }

    }

}
