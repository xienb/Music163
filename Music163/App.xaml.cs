using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Windows;

namespace Music163
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
        }

        private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            var assName = new AssemblyName(args.Name).FullName;
            if (args.Name == "Newtonsoft.Json, Version=12.0.0.0, Culture=neutral, PublicKeyToken=30ad4fe6b2a6aeed")
            {
                var bytes = Music163.Properties.Resources.Newtonsoft_Json;
                return Assembly.Load(bytes);//加载资源文件中的dll,代替加载失败的程序集
            }
            throw new DllNotFoundException(assName);
        }
    }
}
