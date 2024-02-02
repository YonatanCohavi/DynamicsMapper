using System;
using System.Runtime.Serialization;

namespace DynamicsMapper.Abstractions.Mappers
{
    [Serializable]
    internal class PropertyMapperNoFoundException : Exception
    {
        public PropertyMapperNoFoundException()
        {
        }

        public PropertyMapperNoFoundException(string message) : base(message)
        {
        }

        public PropertyMapperNoFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected PropertyMapperNoFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}