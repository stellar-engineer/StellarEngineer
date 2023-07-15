using System;

namespace StellarEngineer
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class EntrypointAttribute: Attribute {}
}