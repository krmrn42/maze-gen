using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace PlayersWorlds.Maps {
    /// <summary>
    /// Provides a source of randomness with the ability to use a specific seed.
    /// </summary>
    public class RandomSource {
        private static int s_instanceCount = 0;
        private readonly Log _log = Log.ToConsole<RandomSource>();
        private readonly Random _random;
        private readonly int _instanceId;

        /// <summary>
        /// Gets the seed used for random number generation.
        /// </summary>
        public int Seed { get; private set; }
        /// <summary>
        /// Optional environment-specific seed for random number generation.
        /// If set, it overrides the default seed.
        /// </summary>
        public static int? EnvRandomSeed { get; set; }

        /// <summary>
        /// Initializes a new instance of the RandomSource class with a
        /// specified seed.
        /// </summary>
        /// <param name="seed">The seed to use for random number generation.
        /// </param>
        protected RandomSource(int seed) {
            _instanceId = Interlocked.Increment(ref s_instanceCount);
            Seed = seed;
            _random = new Random(Seed);
        }

        /// <summary>
        /// Creates a new RandomSource instance using either the
        /// environment-defined seed or a seed based on the current time.
        /// </summary>
        /// <returns>A new instance of RandomSource.</returns>
        public static RandomSource CreateFromEnv() {
            if (EnvRandomSeed.HasValue)
                return new RandomSource(EnvRandomSeed.Value);
            else return new RandomSource(DateTime.Now.Millisecond);
        }

        /// <summary>
        /// Logs the usage of the random source and returns the Random instance.
        /// </summary>
        /// <param name="refName">The reference name of the method calling this
        /// function.</param>
        /// <returns>The Random instance for generating random values.</returns>
        private Random D(string refName) {
            _log.D(4, $"[{_instanceId}] RandomSource.{refName}");
            return _random;
        }

        /// <summary>
        /// Returns a non-negative random integer.
        /// </summary>
        /// <returns>A 32-bit signed integer that is greater than or equal to 0
        /// and less than Int32.MaxValue.</returns>
        public int Next() {
            return D("Next()").Next();
        }

        /// <summary>
        /// Returns a random integer that is within a specified range.
        /// </summary>
        /// <param name="min">The inclusive lower bound of the random number 
        /// returned.</param>
        /// <param name="max">The exclusive upper bound of the random number 
        /// returned. max must be greater than or equal to min.</param>
        /// <returns>A 32-bit signed integer greater than or equal to min and 
        /// less than max.</returns>
        public int Next(int min, int max) {
            return D("Next(int,int)").Next(min, max);
        }

        /// <summary>
        /// Returns a random double that is within a specified range.
        /// </summary>
        /// <param name="min">The inclusive lower bound of the random number 
        /// returned.</param>
        /// <param name="max">The exclusive upper bound of the random number 
        /// returned.</param>
        /// <param name="precision">The precision to use for the random number. 
        /// Defaults to 100.</param>
        /// <returns>A double greater than or equal to min and less than max.
        /// </returns>
        public double Next(double min, double max,
                                  double precision = 100) {
            return (double)(
                Next((int)(min * precision), (int)(max * precision))
                    / precision
            );
        }

        /// <summary>
        /// Returns a random floating-point number that is greater than or equal
        /// to 0.0, and less than 1.0.
        /// </summary>
        /// <returns>A single-precision floating point number that is greater 
        /// than or equal to 0.0 and less than 1.0.</returns>
        public float RandomSingle() {
            return D("RandomSingle()").Next(99999) / 100000f;
        }

        /// <summary>
        /// Fills the elements of a specified array of bytes with random 
        /// numbers.
        /// </summary>
        /// <param name="count">The number of bytes to generate.</param>
        /// <returns>An array of bytes with random values.</returns>
        public byte[] NextBytes(int count) {
            var rndBytes = new byte[count];
            D("NextBytes(int)").NextBytes(rndBytes);
            return rndBytes;
        }

        /// <summary>
        /// Returns a random element from a list.
        /// </summary>
        /// <typeparam name="T">The type of elements in the list.</typeparam>
        /// <param name="items">The list of items to choose from.</param>
        /// <returns>A randomly selected item from the list.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the list is 
        /// empty.</exception>
        public T RandomOf<T>(IList<T> items) {
            return items.Count == 0 ?
                throw new InvalidOperationException(
                    "Cannot get a random item from an empty list") :
                items[D("RandomOf<T>(IList<T>)").Next(items.Count)];
        }

        /// <summary>
        /// Returns a random element from a list or the default value if the 
        /// list is empty.
        /// </summary>
        /// <typeparam name="T">The type of elements in the list.</typeparam>
        /// <param name="items">The list of items to choose from.</param>
        /// <returns>A randomly selected item from the list or default(T) if the
        /// list is empty.</returns>
        public T RandomOrDefaultOf<T>(IList<T> items) {
            if (items.Count == 0) {
                return default;
            }
            return RandomOf(items);
        }

        /// <summary>
        /// Returns a random element from a sequence.
        /// </summary>
        /// <typeparam name="T">The type of elements in the sequence.
        /// </typeparam>
        /// <param name="items">The sequence of items to choose from.</param>
        /// <returns>A randomly selected item from the sequence.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the sequence
        /// is empty.</exception>
        public T RandomOf<T>(ICollection<T> items) =>
            RandomOf(items, items.Count);


        /// <summary>
        /// Returns a random element from a sequence.
        /// </summary>
        /// <typeparam name="T">The type of elements in the sequence.
        /// </typeparam>
        /// <param name="items">The sequence of items to choose from.</param>
        /// <param name="count">The number of items in the sequence.</param>
        /// <returns>A randomly selected item from the sequence.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the sequence
        /// is empty.</exception>
        public T RandomOf<T>(IEnumerable<T> items, int count) {
            return count == 0 ?
                throw new InvalidOperationException(
                    "Cannot get a random item from an empty list") :
                items.ElementAt(D("RandomOf<T>(IEnumerable<T>,int)").Next(count));
        }

        /// <summary>
        /// Returns a random element from a sequence or the default value if the
        /// sequence is empty.
        /// </summary>
        /// <typeparam name="T">The type of elements in the sequence.
        /// </typeparam>
        /// <param name="items">The sequence of items to choose from.</param>
        /// <param name="count">The number of items in the sequence.</param>
        /// <returns>A randomly selected item from the sequence or default(T) if
        /// the sequence is empty.</returns>
        public T RandomOrDefaultOf<T>(IEnumerable<T> items, int count) {
            return count == 0 ? default : items.ElementAt(D("RandomOrDefaultOf(IEnumerable<T>,int)").Next(count));
        }

        /// <inheritdoc/>
        public override string ToString() {
            return $"Random({Seed})";
        }
    }
}
