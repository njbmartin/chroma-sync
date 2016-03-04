using System;
using Neo.IronLua;
using System.IO;
using System.Threading;
using Corale.Colore.Core;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Forms;

namespace Ultrabox.ChromaSync
{
    internal class LuaScripting
    {
        private static readonly object _syncObject = new object();
        private static readonly object debugLock = new object();
        private static List<dynamic> callbacks;
        private static Collection<Thread> scriptThreads;
        private static FileSystemWatcher watcher;

        public static void ReloadScripts()
        {
            CloseScripts();
            LuaThread();
        }

        public static void CloseScripts()
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
            App.c.Uninitialize();
        }

        public static void LuaThread()
        {
            if (watcher == null)
                Watch();

            App.c = Chroma.Instance;
            App.c.Initialize();
            App.NewScriptsContext();
            // WE NEED TO ENSURE CHROMA IS INITIALISED
            callbacks = new List<dynamic>();
            var ms_luaDebug = new LuaStackTraceDebugger();
            var ms_luaCompileOptions = new LuaCompileOptions();
            ms_luaCompileOptions.DebugEngine = ms_luaDebug;
            scriptThreads = new Collection<Thread>();

            string path = @"%appdata%\ChromaSync";
            path = Environment.ExpandEnvironmentVariables(path);

            string scriptsPath = Path.Combine(path, "scripts");
            string packagesPath = Path.Combine(path, "packages");
            if (!Directory.Exists(scriptsPath))
                Directory.CreateDirectory(scriptsPath);


            // Todo: Get all scripts including the packages
            var files = Directory.GetFiles(path, "*.lua", SearchOption.AllDirectories);

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
                        using (Lua l = new Lua())
                        {
                            LuaGlobalPortable g = l.CreateEnvironment();
                            dynamic dg = g;
                            dg.DebugLua = new Func<object, bool>(debug);
                            dg.ConvertInt = new Func<object, int>(convertInt);
                            dg.NewCustom = new Func<string, Color, object>(newCustom);
                            dg.IntToByte = new Func<int, byte>(IntToByte);
                            dg.Headset = Headset.Instance;
                            dg.Keyboard = Keyboard.Instance;
                            dg.Mouse = Mouse.Instance;
                            dg.Keypad = Keypad.Instance;
                            dg.Mousepad = Mousepad.Instance;

                            dg.RegisterForEvents = new Func<string, object, bool>(registerEvents);
                            debug("starting Lua script: " + st);
                            try
                            {

                                LuaChunk compiled = l.CompileChunk(st, ms_luaCompileOptions);
                                var d = g.DoChunk(compiled);
                            }
                            catch (LuaException e)
                            {
                                App.Log.Error(e);
                            }
                            catch (Exception e)
                            {
                                App.Log.Info(e);
                                //Thread.ResetAbort();
                            }
                        }
                    }));
                    scriptThreads.Last().Start();
                }
            }
        }


        private static void MenuItem_Click(object sender, EventArgs e)
        {
            MenuItem s = (MenuItem)sender;
            RegistryKeeper.UpdateReg((string)s.Tag, (!s.Checked).ToString());
            ReloadScripts();
        }

        public static int convertInt(object o)
        {
            return Convert.ToInt32(o);
        }

        public static byte IntToByte(int o)
        {
            return (byte)o;
        }



        public static bool debug(object d)
        {
            var text = DateTime.Now + " - " + d;
            App.Log.Debug(text);
            return true;
        }


        public static void PassThrough(JObject json)
        {
            JObject j = (JObject)json.DeepClone();
            foreach (LuaCallback action in callbacks)
            {
                var name = json["provider"] != null ? json["provider"]["name"].ToString() : json["product"]["name"].ToString();
                if (action.name == name)
                {
                    try
                    {
                        action.callback(j);
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

        public static bool registerEvents(string n, object c)
        {

            callbacks.Add(new LuaCallback { name = n, callback = (Func<object, LuaResult>)c });
            debug("Registered Callback: " + n);
            return true;
        }


        private static void OnChanged(object source, FileSystemEventArgs e)
        {
            Debug.WriteLine("Changed");
            watcher.EnableRaisingEvents = false;
            
            ReloadScripts();
            watcher.EnableRaisingEvents = true;
        }


        private static void Watch()
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
            watcher.Filter = "*.lua";
            Debug.WriteLine("Enabled watch");
            watcher.Created += new FileSystemEventHandler(OnChanged);
            watcher.Changed += new FileSystemEventHandler(OnChanged);
            watcher.EnableRaisingEvents = true;
        }

    }

    public class LuaCallback
    {
        public string name { get; set; }
        public Func<object, LuaResult> callback { get; set; }
    }
}