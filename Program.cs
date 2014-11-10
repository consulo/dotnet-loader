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
        if (assembly == null)
        {
            Console.WriteLine("Assembly {0} is null", path);
            return;
        }

        Type t = assembly.GetType(type);
        if (t == null)
        {
            Console.WriteLine("Type {0} is null. Assembly {1}", type, assembly);
            return;
        }

        MethodInfo methodInfo = t.GetMethod(method, BindingFlags.Static | BindingFlags.NonPublic, null, new Type[] { typeof(String[]) }, null);

        if (methodInfo == null)
        {
            Console.WriteLine("Method 'Main' is not found. Type {0}", type);
            return;
        }

        methodInfo.Invoke(null, parameters);
    }
}

namespace loader
{
    class Program
    {
        static void Main(string[] args)
        {
            String mainExecutableAndConfig = args[0];
            String basePath = args[1];
            String typeName = args[2];

            String mainExecutable = mainExecutableAndConfig;
            String configFile = null;
            if (mainExecutableAndConfig.Contains(";"))
            {
                string[] split = mainExecutableAndConfig.Split(';');
                mainExecutable = split[0];
                configFile = split[1];
            }

            AppDomainSetup setup = new AppDomainSetup();
            setup.ApplicationBase = basePath;
            if (configFile != null)
            {
                setup.ConfigurationFile = configFile;
            }
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
