using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Extensions;
using Xunit.Sdk;

namespace UniqueIdGenerator.Net.Tests
{
    public class GeneratorTests : TestClass
    {
        [Fact]
        public void When_generating_hundred_thousand_ids_with_one_single_generator_then_every_id_is_unique()
        {
            const int number = 100000;

            var ids = GetIds(0, number);

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
        public void When_generating_ten_thousand_ids_with_hundred_different_generators_then_every_id_is_unique()
        {
            const int number = 10000;
            const int machines = 100;
            var lists = new IList<string>[machines];

            Parallel.For(0, machines, m => { lists[m] = GetIds((short)m, number); });

            var unique = new Dictionary<string, int>(StringComparer.Ordinal);

            for (int machineId = 0; machineId < lists.Length; machineId++)
            {
                var list = lists[machineId];

                foreach (var id in list)
                {
                    int previousMachineId;
                    if (unique.TryGetValue(id, out previousMachineId))
                    {
                        var msg = string.Format("Machine {0} and {1} created the same id: {2}", previousMachineId, machineId, id);
                        throw new AssertException(msg);
                    }
                    unique.Add(id, machineId);
                }
            }
        }

        [Fact]
        public void When_changing_only_single_bit_then_the_id_is_generated_correctly()
        {
            var numberOfGenerators = Math.Pow(2, Generator.NumberOfGeneratorIdBits);
            var target = new byte[8];
            var generator = new Generator((short)(numberOfGenerators - 1), DateTime.Today);

            generator.WriteValuesToByteArray(target, 4398046511103, 8191);
            Assert.Equal(new string('1', 64), GetString(target));

            generator.WriteValuesToByteArray(target, 4398046511103, 8190);
            Assert.Equal(new string('1', 63) + "0", GetString(target));

            generator.WriteValuesToByteArray(target, 4398046511102, 8191);
            Assert.Equal(
                new string('1', Generator.NumberOfTimeBits - 1) + "0" + new string('1', 64 - Generator.NumberOfTimeBits),
                GetString(target));

            generator = new Generator((short)(numberOfGenerators - 2), DateTime.Today);
            generator.WriteValuesToByteArray(target, 4398046511103, 8191);
            Assert.Equal(
                new string('1', 63 - Generator.NumberOfSequenceBits) + "0" + new string('1', Generator.NumberOfSequenceBits),
                GetString(target));

            for (int i = 0; i < numberOfGenerators; i++)
            {
                generator = new Generator((short)i, DateTime.Today);
                generator.WriteValuesToByteArray(target, 0, 0);
                Assert.Equal(new string('0', Generator.NumberOfTimeBits), GetString(target).Substring(0, Generator.NumberOfTimeBits));
                Assert.Equal(new string('0', Generator.NumberOfSequenceBits), GetString(target).Substring(64 - Generator.NumberOfSequenceBits, Generator.NumberOfSequenceBits));

                var m = Convert.ToString(i, 2).PadLeft(Generator.NumberOfGeneratorIdBits, '0');
                Assert.Equal(m, GetString(target).Substring(Generator.NumberOfTimeBits, Generator.NumberOfGeneratorIdBits));
            }
        }

        [Fact]
        public void When_iterating_through_all_possible_sequences_then_every_generated_id_is_correct()
        {
            var target = new byte[8];
            var generator = new Generator(0, DateTime.Today);
            var firstPartLength = 64 - Generator.NumberOfSequenceBits;

            for (short i = 0; i < Math.Pow(2, Generator.NumberOfSequenceBits); i++)
            {
                generator.WriteValuesToByteArray(target, 0, i);
                Assert.Equal(new string('0', firstPartLength), GetString(target).Substring(0, firstPartLength));

                var s = Convert.ToString(i, 2).PadLeft(Generator.NumberOfSequenceBits, '0');
                Assert.Equal(s, GetString(target).Substring(firstPartLength, Generator.NumberOfSequenceBits));
            }
        }

        [Fact]
        public void When_iterating_through_all_possible_generators_then_every_generated_id_is_correct()
        {
            var target = new byte[8];

            for (short i = 0; i < Math.Pow(2, Generator.NumberOfGeneratorIdBits); i++)
            {
                var generator = new Generator(i, DateTime.Today);
                generator.WriteValuesToByteArray(target, 0, 0);
                var s = GetString(target);
                Assert.Equal(new string('0', Generator.NumberOfTimeBits), s.Substring(0, Generator.NumberOfTimeBits));
                Assert.Equal(new string('0', Generator.NumberOfSequenceBits), s.Substring(64 - Generator.NumberOfSequenceBits, Generator.NumberOfSequenceBits));

                var m = Convert.ToString(i, 2).PadLeft(Generator.NumberOfGeneratorIdBits, '0');
                Assert.Equal(m, s.Substring(Generator.NumberOfTimeBits, Generator.NumberOfGeneratorIdBits));
            }
        }

        [Fact]
        public void When_converting_from_id_to_long_and_back_then_the_id_is_the_same()
        {
            var tests = new[]
            {
                "AACxgFbAEAA=",
                "AAC3UcJAMAA=",
                "AAC3a4sAMAA=",
                "AADOCnOAEAA=",
                "AAEQBrnAEAA=",
                "AAESfkpAEAA=",
                "AAETZ2AAEAA="
            };

            foreach (var test in tests)
            {
                var l = IdConverter.ToLong(test);
                var s = IdConverter.ToString(l);
                Assert.Equal(test, s);
                Console.WriteLine("{0} => {1}", test, l);
            }
        }

        [Fact]
        public void Performance_test_with_ten_million_unique_ids_with_hundred_different_generators()
        {
            const int number = 100000;
            const int machines = 100;

            var stopwatch = Stopwatch.StartNew();
            Parallel.For(0, machines, m => GetIds((short)m, number));
            stopwatch.Stop();

            Console.WriteLine("Duration to generate {1:n0} ids: {0:n0} ms", stopwatch.ElapsedMilliseconds, number * machines);
            Console.WriteLine("Number of ids generated in 1 ms: {0:n0}", number * machines / stopwatch.ElapsedMilliseconds);
            Console.WriteLine("Number of ids generated in 1 s: {0:n0}", (int)(number * machines / (stopwatch.ElapsedMilliseconds / 1000.0)));
        }

        [Fact]
        public void Performance_test_with_ten_million_unique_ids_with_single_generator()
        {
            const int number = 10000000;

            var stopwatch = Stopwatch.StartNew();
            var ids = GetIds(0, number);
            stopwatch.Stop();

            Console.WriteLine("Duration to generate {1:n0} ids: {0:n0} ms", stopwatch.ElapsedMilliseconds, ids.Count);
            Console.WriteLine("Number of ids generated in 1 ms: {0:n0}", ids.Count / stopwatch.ElapsedMilliseconds);
            Console.WriteLine("Number of ids generated in 1 s: {0:n0}", (int)(ids.Count / (stopwatch.ElapsedMilliseconds / 1000.0)));
        }

        private static string GetString(IEnumerable<byte> bytes)
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
