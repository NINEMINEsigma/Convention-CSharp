using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Convention.RScript
{
    public class RScriptImportClass : ICollection<Type>
    {
        private readonly HashSet<Type> importedTypes = new();
        private readonly Dictionary<string, List<MethodInfo>> cacheImportedFunctions = new();

        public int Count => ((ICollection<Type>)importedTypes).Count;

        public bool IsReadOnly => ((ICollection<Type>)importedTypes).IsReadOnly;

        private void DoAdd(Type type)
        {
            foreach (var method in type.GetMethods(BindingFlags.Static | BindingFlags.Public))
            {
                if (cacheImportedFunctions.ContainsKey(method.Name) == false)
                    cacheImportedFunctions.Add(method.Name, new());
                cacheImportedFunctions[method.Name].Add(method);
            }
        }

        public Type[] GetImports()
        {
            return importedTypes.ToArray();
        }

        public int CountMethod(string methodName)
        {
            return cacheImportedFunctions.TryGetValue(methodName, out var list) ? list.Count : 0;
        }

        public MethodInfo GetMethod(string methodName)
        {
            if (cacheImportedFunctions.TryGetValue(methodName, out var list))
            {
                if (list.Count != 1)
                {
                    throw new AmbiguousMatchException($"Have more than one {methodName} is imported");
                }
                return list[0];
            }
            return null;
        }

        public MethodInfo GetMethodByReturn(string methodName, Type returnType)
        {
            if (cacheImportedFunctions.TryGetValue(methodName, out var list))
            {
                var query = from item in list where item.ReturnType == returnType select item;
                if (query.Count() != 1)
                {
                    throw new AmbiguousMatchException($"Have more than one {methodName} is imported");
                }
                return query.First();
            }
            return null;
        }

        public MethodInfo GetMethod(string methodName, params Type[] parameters)
        {
            static bool Pr(Type[] parameters1, Type[] parameters2)
            {
                if (parameters1.Length != parameters2.Length)
                    return false;
                for (int i = 0, e = parameters1.Length; i != e; i++)
                {
                    if (parameters1[i] != parameters2[i])
                        return false;
                }
                return true;
            }

            if (cacheImportedFunctions.TryGetValue(methodName, out var list))
            {
                var query = from item in list
                            where Pr((from _param in item.GetParameters() select _param.ParameterType).ToArray(), parameters)
                            select item;
                if (query.Count() != 1)
                {
                    throw new AmbiguousMatchException($"Have more than one {methodName} is imported");
                }
                return query.First();
            }
            return null;
        }

        public bool TryAdd(Type type)
        {
            var stats = importedTypes.Add(type);
            if (stats)
            {
                DoAdd(type);
            }
            return stats;
        }

        public void Add(Type type)
        {
            TryAdd(type);
        }

        public IEnumerator<Type> GetEnumerator()
        {
            return ((IEnumerable<Type>)importedTypes).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)importedTypes).GetEnumerator();
        }

        public void Clear()
        {
            ((ICollection<Type>)importedTypes).Clear();
        }

        public bool Contains(Type item)
        {
            return ((ICollection<Type>)importedTypes).Contains(item);
        }

        public void CopyTo(Type[] array, int arrayIndex)
        {
            ((ICollection<Type>)importedTypes).CopyTo(array, arrayIndex);
        }

        public bool Remove(Type item)
        {
            return ((ICollection<Type>)importedTypes).Remove(item);
        }
    }
}
