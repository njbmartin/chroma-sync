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
                    //try {
                    LuaChunk compiled = l.CompileChunk(st, ms_luaCompileOptions);
                    var d = g.DoChunk(compiled);
                    //}catch(LuaException e)
                    //{
                    //   Console.WriteLine(e.FileName + ": "+ e.Line + ": " + e.Message);
                    //}
                }
            }
        }

        private static bool debug(object d)
        {
            var m= Corale.Colore.Razer.Mousepad.Constants.MaxLeds;
            Console.WriteLine(d);
            return true;
        }


        public static void PassThrough(JObject json)
        {
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



