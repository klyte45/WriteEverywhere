using System;
using System.Runtime.Serialization;

namespace WriteEverywhere.Font
{
    [Serializable]
    public class FontCreationException : Exception
    {
        public FontCreationException()
        {
        }

        public FontCreationException(string message) : base(message)
        {
        }

        public FontCreationException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected FontCreationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}