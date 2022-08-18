using Autofac;
//using Palantir.EventBus.Abstractions;
//using Palantir.Identity.IntegrationEvents;
using System.Reflection;
namespace Palantir.Identity.AutofacModules
{
    public class ApplicationModule
        : Autofac.Module
    {

        public string QueriesConnectionString { get; }

        public ApplicationModule()
        {
        }

        protected override void Load(ContainerBuilder builder)
        {

            //builder.RegisterAssemblyTypes(typeof(NotificationIntegrationEvent).GetTypeInfo().Assembly)                .AsClosedTypesOf(typeof(IIntegrationEventHandler<>));

        }
    }
}
