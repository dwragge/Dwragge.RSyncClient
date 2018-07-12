using System;
using System.Windows;
using Autofac;
using Caliburn.Micro;
using Dwragge.RCloneClient.Common.AutoMapper;
using Dwragge.RCloneClient.ManagementUI.ViewModels;
using Dwragge.RCloneClient.Persistence;

namespace Dwragge.RCloneClient.ManagementUI
{
    public class Bootstrapper : BootstrapperBase
    {
        private IContainer _container;

        public Bootstrapper()
        {
            Initialize();
        }

        protected override void Configure()
        {
            var builder = new ContainerBuilder();
            builder.RegisterAutoMapper();
            builder.RegisterType<WindowManager>().As<IWindowManager>().SingleInstance();
            builder.RegisterType<ShellViewModel>().InstancePerDependency();

            _container = builder.Build();
        }

        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            DisplayRootViewFor<ShellViewModel>();
        }

        protected override object GetInstance(Type service, string key)
        {
            if (string.IsNullOrEmpty(key)) return _container.Resolve(service);
            return _container.ResolveKeyed(key, service);
        }
        

        protected override void BuildUp(object instance)
        {
        }
    }
}
