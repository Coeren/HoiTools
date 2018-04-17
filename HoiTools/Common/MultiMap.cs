using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class MultiMap<TKey, TValue> : IDictionary<TKey, TValue>
    {
        public MultiMap() {}

        public ICollection<TKey> Keys => _mmap.Keys;

        public ICollection<TValue> Values
        {
            get
            {
                List<TValue> values = new List<TValue>();
                foreach (var item in _mmap)
                    values.AddRange(item.Value);
                return values;
            }
        }

        public int Count => _mmap.Count;

        public bool IsReadOnly => false;

        public TValue this[TKey key]
        {
            get
            {
                TValue val;
                TryGetValue(key, out val);
                return val;
            }
            set => Add(key, value);
        }

        public bool ContainsKey(TKey key)
        {
            return _mmap.ContainsKey(key);
        }

        public void Add(TKey key, TValue value)
        {
            if (!_mmap.ContainsKey(key))
                _mmap.Add(key, new List<TValue> { value });
            else if (!_mmap[key].Contains(value))
                _mmap[key].Add(value);
        }

        public bool Remove(TKey key)
        {
            return _mmap.Remove(key);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            if (_mmap[key] != null && _mmap[key].Count > 0)
            {
                value = _mmap[key][0];
                return true;
            }

            value = default(TValue);
            return false;
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            Add(item.Key, item.Value);
        }

        public void Clear()
        {
            _mmap.Clear();
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return (_mmap[item.Key] == null) ? false : _mmap[item.Key].Contains(item.Value);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            for (int i = arrayIndex; i < array.Length; i++)
                Add(array[i]);
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            return (_mmap[item.Key] == null) ? false : _mmap[item.Key].Remove(item.Value);
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            List<KeyValuePair <TKey, TValue>> pairs = new List<KeyValuePair<TKey, TValue>>();
            foreach (var item in _mmap)
                foreach (var val in item.Value)
                    pairs.Add(new KeyValuePair<TKey, TValue>(item.Key, val));

            return pairs.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            List<KeyValuePair<TKey, TValue>> pairs = new List<KeyValuePair<TKey, TValue>>();
            foreach (var item in _mmap)
                foreach (var val in item.Value)
                    pairs.Add(new KeyValuePair<TKey, TValue>(item.Key, val));

            return pairs.GetEnumerator();
        }

        public ICollection<TValue> ValueList(TKey key)
        {
            return _mmap[key];
        }

        private Dictionary<TKey, List<TValue>> _mmap = new Dictionary<TKey, List<TValue>>();
    }
}
