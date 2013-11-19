using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace UniqueIdGenerator.Net
{
    /// <summary>
    /// Generates new unique ids based on Twitter's Snowflake algorithm.
    /// This class is not thread-safe.
    /// </summary>
    public class Generator
    {
        private readonly byte[] _buffer = new byte[8];
        private readonly byte[] _timeBytes = new byte[8];
        private readonly byte[] _idBytes = new byte[2];
        private readonly byte[] _sequenceBytes = new byte[2];

        private readonly DateTime _start;

        private short _sequence;
        private long _previousTime;

        /// <summary>
        /// Instantiate the generator. Each Generator should have its own ID, so you can
        /// use multiple Generator instances in a cluster. All generated IDs are unique
        /// provided the start date newer changes. I recommend to choose January 1, 2013.
        /// </summary>
        public Generator(short machineId, DateTime start)
        {
            if (machineId < 0 || machineId > 1023)
            {
                throw new ArgumentException("machineId must be between 0 and 1023.", "machineId");
            }
            if (start > DateTime.Today)
            {
                throw new ArgumentException("start date must not be in the future.", "start");
            }

            CalculateIdBytes(machineId);
            _start = start;
        }

        /// <summary>
        /// Can generate up to 4096 different IDs per millisecond.
        /// </summary>
        public string Next()
        {
            SpinToNextSequence();

            WriteValuesToByteArray(_buffer, _previousTime, _sequence);
            return Convert.ToBase64String(_buffer, Base64FormattingOptions.None);
        }

        internal unsafe void WriteValuesToByteArray(byte[] target, long time, short sequence)
        {
            fixed (byte* arrayPointer = target)
            {
                *(long*)arrayPointer = 0;
            }

            fixed (byte* arrayPointer = _timeBytes)
            {
                *(long*)arrayPointer = time << 22;
            }

            fixed (byte* arrayPointer = _sequenceBytes)
            {
                *(short*)arrayPointer = sequence;
            }

            WriteValuesToByteArray(target, _timeBytes, _idBytes, _sequenceBytes);
        }

        private unsafe void CalculateIdBytes(short id)
        {
            fixed (byte* arrayPointer = _idBytes)
            {
                *(short*)arrayPointer = (short)(id << 4);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void WriteValuesToByteArray(byte[] target, byte[] time, byte[] id, byte[] sequence)
        {
            ////                                                 1234567890123456789012
            //// time: 1111111111111111111111111111111111111111110000
            //// id:                                           0011111111110000
            //// seq:                                                  0000111111111111
            ////
            ////       000000000000000100010111101010100001000010 0000001011 000000000000
            //// pos:  0         1         2         3         4         5         6
            //// byte: 0       1       2       3       4       5       6       7

            target[0] = (byte)(target[0] | time[7]);
            target[1] = (byte)(target[1] | time[6]);
            target[2] = (byte)(target[2] | time[5]);
            target[3] = (byte)(target[3] | time[4]);
            target[4] = (byte)(target[4] | time[3]);
            target[5] = (byte)(target[5] | time[2]);

            target[5] = (byte)(target[5] | id[1]);
            target[6] = (byte)(target[6] | id[0]);

            target[6] = (byte)(target[6] | sequence[1]);
            target[7] = (byte)(target[7] | sequence[0]);
        }

        private void SpinToNextSequence()
        {
            var time = GetTime();

            while (time == _previousTime && _sequence >= 4095)
            {
                Thread.Sleep(TimeSpan.FromTicks(1));
                time = GetTime();
            }

            _sequence = time == _previousTime ? (short)(_sequence + 1) : (short)0;
            _previousTime = time;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private long GetTime()
        {
            return (long)(DateTime.UtcNow - _start).TotalMilliseconds;
        }
    }
}
