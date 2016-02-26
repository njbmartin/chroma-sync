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
            public Assembly Assembly;
        }

        public static void EnablePlugins()
        {
            string path = @"%appdata%\ChromaSync";
            path = Environment.ExpandEnvironmentVariables(path);

            path = Path.Combine(path, "plugins");
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            foreach (string dll in Directory.GetFiles(path, "*.dll"))
            {
                var assembly = Assembly.LoadFile(dll);
                var plugin = new Plugin();
                plugin.Name = dll;
                plugin.Assembly = assembly;
                plugins.Add(plugin);
                Debug.WriteLine("found " + dll);
                Type[] types = assembly.GetTypes();
                foreach (Type type in types)
                {

                    if (type == null) continue;

                    MethodInfo methodInfo = type.GetMethod("AutoStart");
                    if (methodInfo == null) continue;
                    Debug.WriteLine(dll + " has AutoStart");

                    object result = null;
                    ParameterInfo[] parameters = methodInfo.GetParameters();
                    object classInstance = null; //Activator.CreateInstance(type, null);
                    if (parameters.Length == 0)
                        result = methodInfo.Invoke(classInstance, null);
                }
            }
        }
    }
}
