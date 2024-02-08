using System.Collections;
using System.Collections.Generic;

namespace DynamicsMapper.Abstractions
{
    public class DynamicsMappingsTargets : IDictionary<string, string>
    {
        private readonly Dictionary<string, string> _targets;
        public DynamicsMappingsTargets()
        {
            _targets = new Dictionary<string, string>();
        }

        public string this[string key] { get => ((IDictionary<string, string>)_targets)[key]; set => ((IDictionary<string, string>)_targets)[key] = value; }

        public ICollection<string> Keys => ((IDictionary<string, string>)_targets).Keys;

        public ICollection<string> Values => ((IDictionary<string, string>)_targets).Values;

        public int Count => ((ICollection<KeyValuePair<string, string>>)_targets).Count;

        public bool IsReadOnly => ((ICollection<KeyValuePair<string, string>>)_targets).IsReadOnly;

        public void Add(string key, string value)
        {
            ((IDictionary<string, string>)_targets).Add(key, value);
        }

        public void Add(KeyValuePair<string, string> item)
        {
            ((ICollection<KeyValuePair<string, string>>)_targets).Add(item);
        }

        public void Clear()
        {
            ((ICollection<KeyValuePair<string, string>>)_targets).Clear();
        }

        public bool Contains(KeyValuePair<string, string> item)
        {
            return ((ICollection<KeyValuePair<string, string>>)_targets).Contains(item);
        }

        public bool ContainsKey(string key)
        {
            return ((IDictionary<string, string>)_targets).ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<string, string>[] array, int arrayIndex)
        {
            ((ICollection<KeyValuePair<string, string>>)_targets).CopyTo(array, arrayIndex);
        }

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            return ((IEnumerable<KeyValuePair<string, string>>)_targets).GetEnumerator();
        }

        public bool Remove(string key)
        {
            return ((IDictionary<string, string>)_targets).Remove(key);
        }

        public bool Remove(KeyValuePair<string, string> item)
        {
            return ((ICollection<KeyValuePair<string, string>>)_targets).Remove(item);
        }

        public bool TryGetValue(string key, out string value)
        {
            return ((IDictionary<string, string>)_targets).TryGetValue(key, out value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_targets).GetEnumerator();
        }
    }
}
