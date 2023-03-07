using RazorTodo.Abstraction.Services;
using RazorTodo.DAL;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RazorTodo.Service
{
    public class ServiceContainer
    {
        private static ConcurrentDictionary<Type, Type> _TransientServiceTypes { get; set; } = new ConcurrentDictionary<Type, Type>();
        private static ConcurrentDictionary<Type, Type> _SingletonServiceTypes { get; set; } = new ConcurrentDictionary<Type, Type>();
        private static ConcurrentDictionary<Type, object> _SingletonServices { get; set; } = new ConcurrentDictionary<Type, object>();

        private static bool _IsInitialized { get; set; } = false;

        public static T GetTransient<T>()
        {
            if(!_IsInitialized)
            {
                Startup();
            }
            Type interfaceType = typeof(T);
            Type serviceType;
            if(_TransientServiceTypes.TryGetValue(interfaceType, out serviceType))
            {
                var service = Activator.CreateInstance(serviceType);
                return (T)service;
            }
            else
            {
                throw new Exception($"RazorTodo.Service.ServiceContainer.GetTransient: 尚未登錄服務「{interfaceType}」");
            }
        }

        public static T GetSingleton<T>()
        {
            if (!_IsInitialized)
            {
                Startup();
            }
            Type interfaceType = typeof(T);
            Type serviceType;
            if (_SingletonServiceTypes.TryGetValue(interfaceType, out serviceType))
            {
                if(_SingletonServices.Keys.Contains(interfaceType))
                {
                    if (_SingletonServices[interfaceType] == null)
                    {
                        _SingletonServices[interfaceType] = Activator.CreateInstance(serviceType);
                    }
                }
                else
                {
                    _SingletonServices[interfaceType] = Activator.CreateInstance(serviceType);
                }
                return (T)_SingletonServices[interfaceType];
            }
            else
            {
                throw new Exception($"RazorTodo.Service.ServiceContainer.GetTransient: 尚未登錄服務「{interfaceType}」");
            }
        }

        private static void Startup()
        {
            AddTransient<IDbContext>(typeof(RazorTodoContext));
            OnStartup?.Invoke(null, EventArgs.Empty);
            _IsInitialized = true;
        }

        public static event EventHandler OnStartup;
        public static void AddTransient<T>(Type serviceType)
        {
            Type interfaceType = typeof(T);
            _TransientServiceTypes[interfaceType] = serviceType;
        }
        public static void AddSingleton<T>(Type serviceType)
        {
            Type interfaceType = typeof(T);
            _SingletonServiceTypes[interfaceType] = serviceType;
        }

        public static void Restart()
        {
            _IsInitialized = false;
            _TransientServiceTypes = new ConcurrentDictionary<Type, Type>();
            _SingletonServiceTypes = new ConcurrentDictionary<Type, Type>();
            _SingletonServices = new ConcurrentDictionary<Type, object>();
            Startup();
        }
    }
}
