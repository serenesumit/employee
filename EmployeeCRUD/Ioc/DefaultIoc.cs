using Core.Repositories;
using Core.Services;
using Repositories;
using Services;
using System.Collections.Generic;
using Unity;
using Unity.Injection;
using Unity.Interception.ContainerIntegration;
using Unity.Interception.PolicyInjection.MatchingRules;
using Unity.Lifetime;

namespace Ioc
{
    public static class DefaultIoc
    {
        #region Public Properties

        public static IUnityContainer Container
        {
            get
            {
                return container;
            }
        }

        #endregion

        #region Public Methods and Operators

        public static IUnityContainer InitializeContainer(IUnityContainer initializedContainer)
        {
            if (!initialized)
            {
                lock (LockObject)
                {
                    if (!initialized)
                    {
                        container = initializedContainer ?? new UnityContainer();
                        BuildContainer();
                        initialized = true;
                    }
                }
            }

            return container;
        }

        #endregion

        #region Static Fields

        private static readonly object LockObject = new object();

        private static IUnityContainer container;

        private static bool initialized;

        #endregion

        #region Methods

        private static void BuildContainer()
        {
            container.RegisterInstance<IUnityContainer>(container);

            RegisterRepositories();
            RegisterServices();
        }


        private static void ConfigureAop()
        {
            container.AddNewExtension<Interception>();
            var loggingNamespaceMatches = new List<MatchingInfo>();
            loggingNamespaceMatches.Add(new MatchingInfo("EmployeeCRUD.Services.*"));

            loggingNamespaceMatches.Add(new MatchingInfo("EmployeeCRUD.Repositories.*"));
            container.Configure<Interception>()
                .AddPolicy("tracing")
                .AddMatchingRule<NamespaceMatchingRule>(
                    new InjectionConstructor(new InjectionParameter(loggingNamespaceMatches)))
               ;

   
        }


      
        private static void RegisterRepositories()
        {
            ////container.RegisterType<,>();
            container.RegisterType<IUpRepository, UpRepository>();
            container.RegisterType<IFileRepository, AzureBlobStorageFileRepository>();

        }

        private static void RegisterServices()
        {
            container.RegisterType<IEmployeeService, EmployeeService>();
            container.RegisterType<IEmployeeResumeService, EmployeeResumeService>();

        }

        #endregion
    }
}
