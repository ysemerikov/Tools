using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Tools
{
    public static class TypeHelper
    {
        public static FieldInfo[] GetConstants(this Type type)
        {
            return type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                .Where(e => e.IsLiteral && !e.IsInitOnly)
                .ToArray();
        }

        private static IEnumerable<Assembly> GetDependentAssemblies(Assembly analyzedAssembly)
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => a.GetNamesOfReferencedAssemblies().Contains(analyzedAssembly.FullName))
                .Concat(Enumerable.Repeat(analyzedAssembly, 1))
                .Distinct();
        }

        private static IEnumerable<string> GetNamesOfReferencedAssemblies(this Assembly assembly)
        {
            return assembly.GetReferencedAssemblies()
                .Select(assemblyName => assemblyName.FullName);
        }

        public static IEnumerable<Type> GetDescendantTypes(this Type rootType)
        {
            return
                from assembly in GetDependentAssemblies(rootType.Assembly)
                from type in assembly.GetTypes()
                where (type.IsClass || type.IsValueType) && !type.IsAbstract && rootType.IsAssignableFrom(type)
                select type;
        }
    }
}