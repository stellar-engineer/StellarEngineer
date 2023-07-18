
using System.Reflection;

namespace StellarEngineer {
    public static class ModLoader {
        public static void LoadDLL(string path) {
            Assembly assembly = Assembly.LoadFrom(path);
            foreach (var type in assembly.GetTypes()) {
                foreach (var method in type.GetRuntimeMethods()) {
                    if (!method.IsStatic) continue;
                    if (method.ReturnType != typeof(void)) continue;
                    if (method.GetParameters().Length != 0) continue;

                    var attribute = method.GetCustomAttribute<EntrypointAttribute>();

                    if (attribute != null) {
                        method.Invoke(null,null);
                        return;
                    }
                }
            }
        }
    }
}