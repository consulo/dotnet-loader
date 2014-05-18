using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.IO;
using System.Security.Policy;

class ProxyObject : MarshalByRefObject
{
    public Assembly Load(String path)
    {
        return Assembly.LoadFrom(path);
    }

    public void LoadAndCall(String path, String type, String method, Object[] parameters)
    {
        Assembly assembly = Load(path);

        Type t = assembly.GetType(type);

        MethodInfo methodInfo = t.GetMethod("Main", BindingFlags.Static | BindingFlags.NonPublic, null, new Type[] { typeof(String[]) }, null);

        methodInfo.Invoke(null, parameters);
    }
}

namespace loader
{
    class Program
    {
        static void Main(string[] args)
        {
            String mainExecutable = args[0];
            String basePath = args[1];
            String typeName = args[2];

            AppDomainSetup setup = new AppDomainSetup();
            setup.ApplicationBase = basePath;
            AppDomain domain = AppDomain.CreateDomain("ProxyDomain", null, setup);

            ProxyObject proxyObject = (ProxyObject)domain.CreateInstanceFromAndUnwrap(typeof(ProxyObject).Assembly.Location, "ProxyObject");

            String[] parameters = new String[args.Length - 3];
            Array.Copy(args, 3, parameters, 0, parameters.Length);

            String thriftLibrary = AppDomain.CurrentDomain.BaseDirectory + "\\thrift.dll";

            proxyObject.Load(thriftLibrary);
            proxyObject.LoadAndCall(mainExecutable, typeName, "Main", new object[] { parameters });           
        }
    }
}
