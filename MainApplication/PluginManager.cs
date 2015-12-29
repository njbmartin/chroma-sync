using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Ultrabox.ChromaSync
{
    class PluginManager
    {
        private static List<Assembly> allAssemblies = new List<Assembly>();

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
                allAssemblies.Add(assembly);

                Type[] types = assembly.GetTypes();
                foreach (Type type in types)
                {

                    if (type == null) return;

                    MethodInfo methodInfo = type.GetMethod("AutoStart");
                    if (methodInfo == null) return;

                    object result = null;
                    ParameterInfo[] parameters = methodInfo.GetParameters();
                    object classInstance = Activator.CreateInstance(type, null);
                    if (parameters.Length == 0)
                        result = methodInfo.Invoke(classInstance, null);
                }
            }
        }
    }
}
