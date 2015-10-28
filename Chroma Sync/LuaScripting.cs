using System;
using Neo.IronLua;
using System.IO;
using System.Threading;
using Corale.Colore.Core;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace ChromaSync
{
    public class LuaScripting
    {

        public static dynamic dg;
        delegate Func<object, bool> cbs(string json);

        private static List<dynamic> callbacks;
        private static LuaGlobalPortable g;

        public static void LuaThread()
        {
            callbacks = new List<dynamic>();

            var ms_luaDebug = new LuaStackTraceDebugger();
            var ms_luaCompileOptions = new LuaCompileOptions();
            ms_luaCompileOptions.DebugEngine = ms_luaDebug;

            if (!Directory.Exists("scripts\\"))
                return;

            using (Lua l = new Lua())
            {

                g = l.CreateEnvironment();
                dg = g;
                dg.DebugLua = new Func<object, bool>(debug);
                dg.NewCustom = new Func<Corale.Colore.Razer.Mousepad.Effects.Custom>(newCustom);
                dg.RegisterForEvents = new Func<string, object, bool>(registerEvents);
                foreach (string st in Directory.GetFiles("scripts\\", "*_main.lua", SearchOption.AllDirectories))
                {
                    new Thread(() =>
                    { 
                    try {
                        LuaChunk compiled = l.CompileChunk(st, ms_luaCompileOptions);
                        var d = g.DoChunk(compiled);
                    } catch (LuaException e)
                    {
                        debug(e.FileName + ": " + e.Line + ": " + e.Message);
                    }
                    }).Start();
                }
                Console.WriteLine("test");
            }
        }

        public static bool debug(object d)
        {
            Console.WriteLine(d);


            string path = @"scripts\log.txt";
            // This text is added only once to the file.
            if (!File.Exists(path))
            {
                // Create a file to write to.
                using (StreamWriter sw = File.CreateText(path))
                {
                    sw.WriteLine(d);
                }
                return true;
            }

            // This text is always added, making the file longer over time
            // if it is not deleted.
            using (StreamWriter sw = File.AppendText(path))
            {
                sw.WriteLine(d);
            }




            return true;
        }


        public static void PassThrough(JObject json)
        {
            debug("Chroma Sync received data: " + json.ToString());
            foreach (LuaCallback action in callbacks)
            {
                if(action.name == json["provider"]["name"].ToString()) action.callback(json);
            }
        }

        public static Corale.Colore.Razer.Mousepad.Effects.Custom newCustom()
        {
            return new Corale.Colore.Razer.Mousepad.Effects.Custom(new Color());
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



