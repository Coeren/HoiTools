﻿using System;
using System.Collections.Generic;

namespace Common
{
    [Serializable]
    public class ConsistencyException : Exception
    {
        public ConsistencyException(string message) : base(message) { }
        public ConsistencyException(string message, Exception inner) : base(message, inner) { }
        protected ConsistencyException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

    public interface IConsistencyChecker
    {
        void CheckConsistency(); // throw ConsistencyException
    }

    public static class Extenstions
    {
        public static TValue GetValue<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue def = default(TValue))
        {
            return dictionary.ContainsKey(key) ? dictionary[key] : def;
        }

        // Does not work with [flags]
        public static bool IsValid<T>(this T en)
        {
            return en != null && Enum.IsDefined(typeof(T), en);
        }
    }
}