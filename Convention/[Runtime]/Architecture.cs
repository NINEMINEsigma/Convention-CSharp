using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.VisualBasic;

namespace Convention
{
    // Interface

    public interface ISignal
    {

    }

    public interface IModel
    {
        string Save();
        void Load(string data);
    }

    public interface IConvertable<T>
    {
        T ConvertTo();
    }

    public interface IConvertModel<T>
        : IModel, IConvertable<T>
    {
    }

    // Instance

    public class SingletonModel<T>: IModel
    {
        private static T InjectInstance = default;

        public static T Instance
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

        public T Data => InjectInstance;

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

        public static implicit operator T(SingletonModel<T> _) => InjectInstance;
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

        public static Type LoadFromFormat(string data)
        {
            var keys = data.Split("::");
            Assembly asm = null;
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

        public static Type LoadFromFormat(string data, out Exception exception)
        {
            exception = null;
            var keys = data.Split("::");
            Assembly asm = null;
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

        internal static void InternalReset()
        {
            // Register System
            RegisterHistory.Clear();
            UncompleteTargets.Clear();
            Completer.Clear();
            Dependences.Clear();
            Childs.Clear();
            // Event Listener
            SignalListener.Clear();
            // Linear Chain for Dependence
            TimelineQuenes.Clear();
            TimelineContentID = 0;
        }

        #region Objects Registered

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

        private static readonly HashSet<Type> RegisterHistory = new();
        private static readonly Dictionary<Type, object> UncompleteTargets = new();
        private static readonly Dictionary<Type, Action> Completer = new();
        private static readonly Dictionary<Type, DependenceModel> Dependences = new();
        private static readonly Dictionary<Type, object> Childs = new();

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

        private static bool InternalRegisteringComplete(out HashSet<Type> InternalUpdateBuffer)
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

        private static void InternalRegisteringUpdate(HashSet<Type> InternalUpdateBuffer)
        {
            foreach (var complete in InternalUpdateBuffer)
            {
                Dependences.Remove(complete);
            }
            foreach (var complete in InternalUpdateBuffer)
            {
                Completer[complete]();
                Completer.Remove(complete);
            }
            foreach (var complete in InternalUpdateBuffer)
            {
                Childs.Add(complete, UncompleteTargets[complete]);
                UncompleteTargets.Remove(complete);
            }
        }

        public static Registering Register(Type slot, object target, Action completer, params Type[] dependences)
        {
            if (RegisterHistory.Add(slot) == false)
            {
                throw new InvalidOperationException("Illegal duplicate registrations");
            }
            Completer[slot] = completer;
            UncompleteTargets[slot] = target;
            Dependences[slot] = new DependenceModel(from dependence in dependences where dependence != slot select new TypeQuery(dependence));
            while (InternalRegisteringComplete(out var buffer))
                InternalRegisteringUpdate(buffer);
            return new Registering(slot);
        }

        public static Registering Register<T>(T target, Action completer, params Type[] dependences) => Register(typeof(T), target!, completer, dependences);

        public static bool Contains(Type type) => Childs.ContainsKey(type);

        public static bool Contains<T>() => Contains(typeof(T));

        public static object InternalGet(Type type) => Childs[type];

        public static object Get(Type type) => InternalGet(type);

        public static T Get<T>() => (T)Get(typeof(T));

        #endregion

        #region Signal & Update

        private static readonly Dictionary<Type, HashSet<Action<ISignal>>> SignalListener = new();

        public class Listening
        {
            private readonly Action<ISignal> action;
            private readonly Type type;

            public Listening(Action<ISignal> action, Type type)
            {
                this.action = action;
                this.type = type;
            }

            public void StopListening()
            {
                if (SignalListener.TryGetValue(type, out var actions))
                    actions.Remove(action);
            }
        }

        public static Listening AddListener<Signal>(Type slot, Action<Signal> listener) where Signal : ISignal
        {
            if (SignalListener.ContainsKey(slot) == false)
                SignalListener.Add(slot, new());
            void action(ISignal x)
            {
                if (x is Signal signal)
                    listener(signal);
            }
            Listening result = new(action, slot);
            SignalListener[slot].Add(action);
            return result;
        }

        public static void SendMessage(Type slot, ISignal signal)
        {
            if(SignalListener.TryGetValue(slot,out var actions))
            {
                foreach (var action in actions)
                {
                    action(signal);
                }
            }
        }

        public static void SendMessage<Signal>(Signal signal) where Signal : ISignal => SendMessage(signal.GetType(), signal);

        #endregion

        #region Timeline/Chain & Update

        private class TimelineQueneEntry
        {
            public Func<bool> predicate;
            public List<Action> actions = new();
        }

        private class Timeline
        {
            public Dictionary<Func<bool>, int> PredicateMapper = new();
            public List<TimelineQueneEntry> Quene = new();
            public int Context = 0;
        }

        private static Dictionary<int, Timeline> TimelineQuenes = new();
        private static int TimelineContentID = 0;

        public static int CreateTimeline()
        {
            TimelineQuenes.Add(TimelineContentID++, new());
            return TimelineQuenes.Count;
        }

        public static void AddStep(int timelineId, Func<bool> predicate,params Action[] actions)
        {
            var timeline = TimelineQuenes[timelineId];
            if (timeline.PredicateMapper.TryGetValue(predicate, out var time))
            {
                timeline.Quene[time].actions.AddRange(actions);
            }
            else
            {
                time = timeline.Quene.Count;
                timeline.PredicateMapper.Add(predicate, time);
                timeline.Quene.Add(new()
                {
                    predicate = predicate,
                    actions = actions.ToList()
                });
            }
        }

        public static void UpdateTimeline()
        {
            for (bool stats = true; stats;)
            {
                stats = false;
                foreach (var pair in TimelineQuenes)
                {
                    var timeline = pair.Value;
                    if (timeline.Quene[timeline.Context].predicate())
                    {
                        stats = true;
                        foreach (var action in timeline.Quene[timeline.Context].actions)
                        {
                            action();
                        }
                        timeline.Context++;
                    }
                }
            }
        }

        public static void ResetTimelineContext(int timelineId)
        {
            TimelineQuenes[timelineId].Context = 0;
        }

        #endregion
    }
}
