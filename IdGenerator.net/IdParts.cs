using System;
using System.Linq;

namespace UniqueIdGenerator.Net
{
    public struct IdParts
    {
        private readonly string _bits;
        private readonly TimeSpan _time;
        private readonly short _generatorId;
        private readonly long _sequence;

        public IdParts(string id)
        {
            var bytes = Convert.FromBase64String(id);
            _bits = string.Concat(bytes.Select(y => Convert.ToString((byte)y, 2).PadLeft(8, '0')));
            _generatorId = Convert.ToInt16(_bits.Substring(42, 10), 2);
            _sequence = Convert.ToInt16(_bits.Substring(52, 12), 2);
            _time = TimeSpan.FromMilliseconds(Convert.ToInt64(_bits.Substring(0, 42), 2));
        }

        public string Bits { get { return _bits; } }
        public TimeSpan Time { get { return _time; } }
        public short GeneratorId { get { return _generatorId; } }
        public long Sequence { get { return _sequence; } }
    }
}