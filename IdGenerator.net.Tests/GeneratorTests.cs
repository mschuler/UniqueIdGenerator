using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace IdGenerator.Net.Tests
{
    public class GeneratorTests
    {
        [Fact]
        public void GetThousands()
        {
            const int Number = 100000;

            var stopwatch = Stopwatch.StartNew();
            var ids = GetIds(0, Number);
            stopwatch.Stop();

            Console.WriteLine("Duration to generate {1} ids: {0} ms", stopwatch.ElapsedMilliseconds, ids.Count);
            Console.WriteLine("Number of ids generated in 1 ms: {0}", ids.Count / stopwatch.ElapsedMilliseconds);
            Console.WriteLine();

            var unique = new HashSet<string>(StringComparer.Ordinal);
            foreach (var id in ids)
            {
                Assert.False(unique.Contains(id), id);
                unique.Add(id);
            }

            for (int i = 0; i < 100; i++)
            {
                Console.WriteLine(ids[i]);
            }
        }

        [Fact]
        public void GetWithHundredMachines()
        {
            const int Number = 10000;
            const int Machines = 100;
            var lists = new IList<string>[Machines];

            var stopwatch = Stopwatch.StartNew();
            Parallel.For(0, Machines, m => { lists[m] = GetIds((short)m, Number); });
            stopwatch.Stop();

            var ids = lists.SelectMany(l => l).ToList();

            Console.WriteLine("Duration to generate {1} ids: {0} ms", stopwatch.ElapsedMilliseconds, ids.Count);
            Console.WriteLine("Number of ids generated in 1 ms: {0}", ids.Count / stopwatch.ElapsedMilliseconds);
            Console.WriteLine();

            var unique = new HashSet<string>(StringComparer.Ordinal);
            foreach (var id in ids)
            {
                Assert.False(unique.Contains(id), id);
                unique.Add(id);
            }
        }

        [Fact]
        public void GetValue()
        {
            var target = new byte[8];
            var generator = new Generator(1023, DateTime.Today);

            generator.WriteValuesToByteArray(target, 4398046511103, 4095);
            Assert.Equal("1111111111111111111111111111111111111111111111111111111111111111", GetString(target));

            generator.WriteValuesToByteArray(target, 4398046511103, 4094);
            Assert.Equal("1111111111111111111111111111111111111111111111111111111111111110", GetString(target));

            generator.WriteValuesToByteArray(target, 4398046511102, 4095);
            Assert.Equal("1111111111111111111111111111111111111111101111111111111111111111", GetString(target));

            generator = new Generator(1022, DateTime.Today);
            generator.WriteValuesToByteArray(target, 4398046511103, 4095);
            Assert.Equal("1111111111111111111111111111111111111111111111111110111111111111", GetString(target));
        }

        [Fact]
        public void CollisionTest()
        {
            Func<string, string> bits = id => GetString(Convert.FromBase64String(id));
            Func<string, long> ms = id => Convert.ToInt64(GetString(Convert.FromBase64String(id)).Substring(0, 42), 2);
            Func<string, short> mid = id => Convert.ToInt16(GetString(Convert.FromBase64String(id)).Substring(42, 10), 2);
            Func<string, short> seq = id => Convert.ToInt16(GetString(Convert.FromBase64String(id)).Substring(52, 12), 2);
            Func<long, string> ts = l => TimeSpan.FromMilliseconds(l).ToString("g", CultureInfo.InvariantCulture);

            Action<string> print = id => Console.WriteLine("{0} - {1} - {2} - {3}", bits(id), ts(ms(id)), mid(id), seq(id));

            print("AACxgFbAEAA=");
            print("AAC3UcJAMAA=");
            print("AAC3a4sAMAA=");
        }

        private static string GetString(byte[] bytes)
        {
            return string.Concat(bytes.SelectMany(y => Convert.ToString(y, 2).PadLeft(8, '0')));
        }

        private IList<string> GetIds(short machineId, int number)
        {
            var generator = new Generator(machineId, DateTime.Today);
            var ids = new List<string>(number);

            for (int i = 0; i < number; i++)
            {
                ids.Add(generator.Next());
            }

            return ids;
        }
    }
}
