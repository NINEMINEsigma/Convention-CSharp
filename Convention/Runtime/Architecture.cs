using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Convention
{
    // Interface

    public interface IModel
    {
        string Save();
        void Load(string data);
    }

    public interface IConvertModel<T> 
        : IModel
    {
        T ConvertTo();
    }

    // Instance

    public class SingletonModel<T>: IModel
    {
        private static T? InjectInstance = default;

        public static T? Instance
        {
            get => InjectInstance;
            set
            {
                if (value == null && InjectInstance == null)
                    return;
                if (InjectInstance == null || InjectInstance.Equals(value) == false)
                {
                    InjectInstance = value;
                }
            }
        }

        public T? Data => InjectInstance;

        void IModel.Load(string data)
        {
            if(typeof(T).GetInterfaces().Contains(typeof(IModel)))
            {
                typeof(T).GetMethod(nameof(IModel.Load))!.Invoke(Instance, new object[] { data });
            }     
            throw new InvalidOperationException();
        }

        string IModel.Save()
        {
            if (typeof(T).GetInterfaces().Contains(typeof(IModel)))
            {
                return (string)typeof(T).GetMethod(nameof(IModel.Save))!.Invoke(Instance, Array.Empty<object>())!;
            }
            throw new InvalidOperationException();
        }

        public static implicit operator T?(SingletonModel<T> _) => InjectInstance;
    }

    public class DependenceModel
        : IConvertModel<bool>, IEnumerable<IConvertModel<bool>>
    {
        private readonly IConvertModel<bool>[] queries;

        public DependenceModel(params IConvertModel<bool>[] queries)
        {
            this.queries = queries;
        }
        public DependenceModel(IEnumerable<IConvertModel<bool>> queries)
        {
            this.queries = queries.ToArray();
        }

        public bool ConvertTo()
        {
            foreach (var query in queries)
            {
                if (query.ConvertTo() == false)
                    return false;
            }
            return true;
        }

        public IEnumerator<IConvertModel<bool>> GetEnumerator()
        {
            return ((IEnumerable<IConvertModel<bool>>)this.queries).GetEnumerator();
        }

        public virtual void Load(string data)
        {
            throw new NotImplementedException();
        }

        public virtual string Save()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.queries.GetEnumerator();
        }
    }

    public static class Architecture
    {
        public static string FormatType(Type type)
        {
            return type.Assembly + "::" + type.FullName;
        }

        public static Type? LoadFromFormat(string data)
        {
            var keys = data.Split("::");
            Assembly? asm = null;
            try
            {
                asm = Assembly.LoadFrom(keys[0]);
                return asm.GetType(keys[1]);
            }
            catch
            {
                return null;
            }
        }

        public static Type? LoadFromFormat(string data, out Exception? exception)
        {
            exception = null;
            var keys = data.Split("::");
            Assembly? asm = null;
            try
            {
                asm = Assembly.LoadFrom(keys[0]);
                return asm.GetType(keys[1]);
            }
            catch (Exception ex)
            {
                exception = ex;
                return null;
            }
        }

        private class TypeQuery
            : IConvertModel<bool>
        {
            private Type queryType;

            public TypeQuery(Type queryType)
            {
                this.queryType = queryType;
            }

            public bool ConvertTo()
            {
                return Architecture.Childs.ContainsKey(queryType);
            }

            public void Load(string data)
            {
                throw new NotImplementedException();
            }

            public string Save()
            {
                throw new NotImplementedException();
            }
        }

        private static HashSet<Type> RegisterHistory = new();
        private static Dictionary<Type, object> UncompleteTargets = new();
        private static Dictionary<Type, Action> Completer = new();
        private static Dictionary<Type, DependenceModel> Dependences = new();
        private static Dictionary<Type, object> Childs = new();

        public class Registering : IConvertModel<bool>
        {
            private readonly Type registerSlot;

            public Registering(Type registerSlot)
            {
                this.registerSlot = registerSlot;
            }

            public bool ConvertTo()
            {
                return Architecture.Childs.ContainsKey(registerSlot);
            }

            public void Load(string data)
            {
                throw new InvalidOperationException($"Cannt use {nameof(Registering)} to load type");
            }

            public string Save()
            {
                return $"{FormatType(registerSlot)}[{ConvertTo()}]";
            }
        }

        private static bool InternalComplete(out HashSet<Type> InternalUpdateBuffer)
        {
            InternalUpdateBuffer = new();
            bool result = false;
            foreach (var dependence in Dependences)
            {
                if (dependence.Value.ConvertTo())
                {
                    InternalUpdateBuffer.Add(dependence.Key);
                    result = true;
                }
            }
            return result;
        }

        private static void InternalUpdate(HashSet<Type> InternalUpdateBuffer)
        {
            foreach (var complete in InternalUpdateBuffer)
            {
                Dependences.Remove(complete);

            }
            foreach (var complete in InternalUpdateBuffer)
            {
                Childs.Add(complete, UncompleteTargets[complete]);
                UncompleteTargets.Remove(complete);
            }
            InternalUpdateBuffer.Clear();
        }

        public static Registering Register(Type slot, object target, Action completer, params Type[] dependences)
        {
            if (RegisterHistory.Add(slot) == false)
            {
                throw new InvalidOperationException("Illegal duplicate registrations");
            }
            Completer[slot] = completer;
            UncompleteTargets[slot] = target;
            Dependences[slot] = new DependenceModel(from dependence in dependences select new TypeQuery(dependence));
            while (InternalComplete(out var buffer))
                InternalUpdate(buffer);
            return new Registering(slot);
        }

        public static Registering Register<T>(T target, Action completer, params Type[] dependences) => Register(typeof(T), target!, completer, dependences);

        public static bool Contains(Type type) => Childs.ContainsKey(type);

        public static bool Contains<T>() => Contains(typeof(T));

        public static object InternalGet(Type type) => Childs[type];

        public static object Get(Type type) => InternalGet(type);

        public static T Get<T>() => (T)Get(typeof(T));
    }
}
