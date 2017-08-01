using System;
using System.Linq;

namespace UniqueIdGenerator.NetCore
{
    public struct IdParts
    {
        private readonly string _bits;
        private readonly short _generatorId;
        private readonly long _sequence;
        private readonly TimeSpan _time;

        public IdParts(string id)
        {
            var bytes = Convert.FromBase64String(id);
            _bits = string.Concat(bytes.SelectMany(y => Convert.ToString((byte)y, 2).PadLeft(8, '0')));
            _generatorId = Convert.ToInt16(_bits.Substring(42, 10), 2);
            _sequence = Convert.ToInt16(_bits.Substring(52, 12), 2);
            _time = TimeSpan.FromMilliseconds(Convert.ToInt64(_bits.Substring(0, 42), 2));
        }

        public string Bits { get { return _bits; } }
        public short GeneratorId { get { return _generatorId; } }
        public long Sequence { get { return _sequence; } }
        public TimeSpan Time { get { return _time; } }
    }
}