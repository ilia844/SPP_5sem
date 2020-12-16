using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DILib
{
    public class DiConfig
    {
        private readonly Dictionary<Type, IGenerator> Defined = new Dictionary<Type, IGenerator>();


        public object Get(Type type)
        {
            if (typeof(IEnumerable).IsAssignableFrom(type))
            {
                var genericArgument = type.GetGenericArguments()[0];
                try
                {
                    return GetAll(genericArgument);
                }
                catch (TypeNotDefinedException)
                {
                }
            }

            //Check if needed type is defined
            if (Defined.ContainsKey(type))
            {
                return Defined[type].Generate();
            }

            //Search for inherited types
            foreach (var definedType in Defined.Keys)
            {
                if (type.IsAssignableFrom(definedType))
                {
                    return Defined[definedType].Generate();
                }
            }

            throw new TypeNotDefinedException();
        }

        private IEnumerable GetAll(Type type)
        {
            var constructed = typeof(List<>).MakeGenericType(type);
            var list = (IList) Activator.CreateInstance(constructed);
            if (Defined.ContainsKey(type))
            {
                list.Add(Defined[type].Generate());
            }

            foreach (var definedType in Defined.Keys)
            {
                if (type.IsAssignableFrom(definedType))
                {
                    list.Add(Defined[definedType].Generate());
                }
            }

            if (list.Count > 0)
            {
                return list;
            }

            else
            {
                throw new TypeNotDefinedException();
            }
        }

        public void AddGenerator<T>(IGenerator generator)
        {
            if (!Defined.ContainsKey(typeof(T)))
                Defined.Add(typeof(T), generator);
            else
                throw new AlreadyDefinedException();
        }

        public void AddSingleGenerator<T>()
        {
            var generator = new Single(GenerateCreateFromConstructor(GetConstructor<T>()));
            AddGenerator<T>(generator);
        }

        public void AddFabricGenerator<T>()
        {
            var generator = new Fabric(GenerateCreateFromConstructor(GetConstructor<T>()));
            AddGenerator<T>(generator);
        }

        private Create GenerateCreateFromConstructor(ConstructorInfo constructor)
        {
            return () =>
            {
                return constructor.Invoke(constructor
                    .GetParameters()
                    .Select(info => info.ParameterType)
                    .Select(parameter => Get(parameter))
                    .ToArray()
                );
            };
        }

        private static ConstructorInfo GetConstructor<T>()
        {
            var type = typeof(T);

            var constructors = type.GetConstructors();
            if (constructors.Length > 0)
            {
                return constructors[0];
            }

            else
            {
                throw new NotFitConstructorException();
            }
        }
    }
}