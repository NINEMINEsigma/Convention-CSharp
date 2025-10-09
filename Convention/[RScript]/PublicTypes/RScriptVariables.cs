using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Convention.RScript
{
    public struct RScriptVariableEntry
    {
        public Type type;
        public object data;
    }
    public class RScriptVariables : IDictionary<string, RScriptVariableEntry>
    {
        private readonly Dictionary<string, Stack<RScriptVariableEntry>> variables = new();

        public RScriptVariableEntry this[string key]
        {
            get
            {
                return variables[key].Peek();
            }
            set
            {
                var current = variables[key].Peek();
                if (current.type != value.type)
                    throw new ArgumentException($"current type is {current.type}, but setter.value is {value.type}");
                variables[key].Pop();
                variables[key].Push(value);
            }
        }

        public ICollection<string> Keys => variables.Keys;

        public ICollection<RScriptVariableEntry> Values => (from item in variables.Values select item.Peek()).ToArray();

        public int Count => variables.Count;

        public bool IsReadOnly => false;

        public void Add(string key, RScriptVariableEntry value)
        {
            if (variables.ContainsKey(key) == false)
                variables.Add(key, new());
            variables[key].Push(value);
        }

        public void Add(KeyValuePair<string, RScriptVariableEntry> item)
        {
            Add(item.Key, item.Value);
        }

        public void ClearAllLayers()
        {
            variables.Clear();
        }

        /// <summary>
        /// <see cref="ClearAllLayers"/>
        /// </summary>
        public void Clear()
        {
            ClearAllLayers();
        }

        public bool Contains(KeyValuePair<string, RScriptVariableEntry> item)
        {
            if (variables.TryGetValue(item.Key, out var items))
            {
                var current = items.Peek();
                return current.data == item.Value.data;
            }
            return false;
        }

        public bool ContainsKey(string key)
        {
            return variables.ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<string, RScriptVariableEntry>[] array, int arrayIndex)
        {
            foreach (var (key, items) in variables)
            {
                array[arrayIndex++] = new(key, items.Peek());
            }
        }

        public IEnumerator<KeyValuePair<string, RScriptVariableEntry>> GetEnumerator()
        {
            return (from items in variables select new KeyValuePair<string, RScriptVariableEntry>(items.Key, items.Value.Peek())).GetEnumerator();
        }

        public bool Remove(string key)
        {
            if (variables.TryGetValue(key, out var items))
            {
                items.Pop();
                if (items.Count == 0)
                {
                    variables.Remove(key);
                }
                return true;
            }
            return false;
        }

        public bool Remove(KeyValuePair<string, RScriptVariableEntry> item)
        {
            if (variables.TryGetValue(item.Key, out var items))
            {
                if (item.Value.data == items.Peek().data)
                {
                    items.Pop();
                    if (items.Count == 0)
                    {
                        variables.Remove(item.Key);
                    }
                    return true;
                }
            }
            return false;
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out RScriptVariableEntry value)
        {
            if (variables.TryGetValue(key, out var items))
            {
                value = items.Peek();
                return true;
            }
            value = default;
            return false;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void SetValue(string varName, object value)
        {
            var top = variables[varName].Pop();
            top.data = value;
            variables[varName].Push(top);
        }
    }
}
