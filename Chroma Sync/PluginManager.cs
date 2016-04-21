using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Ultrabox.ChromaSync
{
    class PluginManager
    {

        public static List<Plugin> plugins = new List<Plugin>();

        public class Plugin
        {
            public string Name;
            public string Description;
            public Version Version;
            public Assembly Assembly;
            public bool Enabled;
            public MethodInfo AutoStart;
            public MethodInfo RequestStart;
            public MethodInfo RequestStop;

        }

        public static void StopPlugins()
        {
            foreach (Plugin p in plugins)
            {
                //RunMethod(p.RequestStop);
            }
        }

        public static void StartPlugins()
        {
            foreach (Plugin p in plugins)
            {
                RunMethod(p.RequestStart);
            }
        }

        public static void LoadPlugins()
        {
            string path = @"%appdata%\ChromaSync";
            path = Environment.ExpandEnvironmentVariables(path);

            path = Path.Combine(path, "plugins");
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            foreach (string dll in Directory.GetFiles(path, "*.dll"))
            {
                Byte[] dllBytes= File.ReadAllBytes(dll);
                var assembly = Assembly.Load(dllBytes);
                dllBytes = null;
                var plugin = new Plugin();
                plugin.Name = assembly.GetName().Name;
                plugin.Version = assembly.GetName().Version;
                                                    
                plugin.Assembly = assembly;

                Debug.WriteLine("found " + dll);
                Type[] types = assembly.GetExportedTypes();
                foreach (Type type in types)
                {

                    if (type == null) continue;
                    // Ensure the plugin is for Chroma Sync
                    //Debug.WriteLine(type.FullName);
                    var autostart = type.GetMethod("AutoStart");
                    if (autostart != null)
                    {
                        plugin.AutoStart = autostart;
                        Debug.WriteLine(dll + " has AutoStart");
                        //RunMethod(plugin.AutoStart);

                    }
                        
                    var requestStop = type.GetMethod("RequestStop");
                    if (requestStop != null)
                        plugin.RequestStop = requestStop;


                }
                plugins.Add(plugin);
            }

            foreach(var plugin in plugins)
            {
                RunMethod(plugin.RequestStart);
            }
        }

        private static void RunMethod(MethodInfo m)
        {
            if (m == null) return;

            object result = null;
            ParameterInfo[] parameters = m.GetParameters();
            object classInstance = null; //Activator.CreateInstance(type, null);
            if (parameters.Length == 0)
                result = m.Invoke(null, null);
        }

    }
}
