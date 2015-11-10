using System;
using Neo.IronLua;
using System.IO;
using System.Threading;
using Corale.Colore.Core;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System.Diagnostics;

namespace ChromaSync
{
    public class LuaScripting
    {

        private static readonly object _syncObject = new object();
        private static readonly object debugLock = new object();
        private static List<dynamic> callbacks;

        public static void LuaThread()
        {
            // WE NEED TO ENSURE CHROMA IS INITIALISED
            var c = Chroma.Instance;
            callbacks = new List<dynamic>();
            var ms_luaDebug = new LuaStackTraceDebugger();
            var ms_luaCompileOptions = new LuaCompileOptions();
            ms_luaCompileOptions.DebugEngine = ms_luaDebug;

            EventHook.MouseHook.MouseAction += new EventHandler(Event);

            if (!Directory.Exists("scripts\\"))
                return;
            foreach (string st in Directory.GetFiles("scripts\\", "*_main.lua", SearchOption.AllDirectories))
            {
                new Thread(() =>
                {
                    using (Lua l = new Lua())
                    {
                        LuaGlobalPortable g = l.CreateEnvironment();
                        dynamic dg = g;
                        dg.DebugLua = new Func<object, bool>(debug);
                        dg.ConvertInt = new Func<JValue, int>(convertInt);
                        dg.NewCustom = new Func<string, Color, object>(newCustom);
                        dg.Keyboard = Keyboard.Instance;
                        dg.Mouse = Mouse.Instance;
                        dg.Keypad = Keypad.Instance;
                        dg.Mousepad = Mousepad.Instance;

                        dg.RegisterForEvents = new Func<string, object, bool>(registerEvents);
                        try
                        {
                            LuaChunk compiled = l.CompileChunk(st, ms_luaCompileOptions);
                            var d = g.DoChunk(compiled);
                        }
                        catch (LuaException e)
                        {
                            debug(e.FileName + ": " + e.Line + ": " + e.Message);
                        }
                    }
                }).Start();
            }
        }

        public static int convertInt(JValue o)
        {
            return o.ToObject<int>();
        }



        public static bool debug(object d)
        {
            var text = DateTime.Now + " - " + d;
            lock (debugLock)
            {
                Debug.WriteLine(text);


                string path = @"scripts\log.txt";
                // This text is added only once to the file.
                if (!File.Exists(path))
                {
                    // Create a file to write to.
                    using (StreamWriter sw = File.CreateText(path))
                    {
                        sw.WriteLine(text);
                        sw.Close();
                    }
                    return true;
                }

                // This text is always added, making the file longer over time
                // if it is not deleted.
                using (StreamWriter sw = File.AppendText(path))
                {
                    sw.WriteLine(text);
                    sw.Close();
                }
            }
            return true;
        }


        public static void PassThrough(JObject json)
        {

            foreach (LuaCallback action in callbacks)
            {
                var name = json["provider"] != null ? json["provider"]["name"].ToString() : json["product"]["name"].ToString();
                if (action.name == name)
                {
                    try
                    {
                        action.callback(json);
                        debug("Data passed to " + action.name);
                    }
                    catch (Exception e)
                    {
                        debug(e);
                        debug("Exception: " + e.StackTrace);
                    }
                }

            }
        }


        public static void Event(object sender, EventArgs e)
        {

            var eventHook = (EventHook.MouseHook.MouseData)sender;
            foreach (LuaCallback action in callbacks)
            {
                var name = "MouseEvents";
                if (action.name == name)
                {
                    try
                    {
                        action.callback(eventHook);
                        debug("Data passed to " + action.name);
                    }
                    catch (Exception ex)
                    {
                        debug(ex);
                        debug("Exception: " + ex.StackTrace);
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
            debug("Registered Callback: " + n);
            callbacks.Add(new LuaCallback { name = n, callback = (Func<object, LuaResult>)c });
            return true;
        }

    }

    public class LuaCallback
    {
        public string name { get; set; }
        public Func<object, LuaResult> callback { get; set; }
    }


}



