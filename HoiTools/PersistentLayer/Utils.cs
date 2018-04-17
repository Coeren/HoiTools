using System;

namespace PersistentLayer
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
}