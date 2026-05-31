using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace STSLite.Core.Random
{
    public class Rng
    {
        private readonly System.Random _random;

        public int Counter { get; private set; }
        public uint Seed { get; }

        public Rng(uint seed = 0u, int counter = 0)
        {
            Seed = seed;
            Counter = counter;
            _random = new System.Random((int)seed);
            FastForwardCounter(counter);
        }

        public Rng(uint seed, string name)
            : this(seed + (uint)StringHelper.GetDeterministicHashCode(name))
        {
        }

        public void FastForwardCounter(int count)
        {
            if (Counter > count)
            {
                throw new System.ArgumentException("Counter cannot be fast-forwarded backwards.");
            }

            while (Counter < count)
            {
                _random.Next();
                Counter++;
            }
        }

        public bool NextBool()
        {
            Counter++;
            return _random.Next(2) == 0;
        }

        public int NextInt(int maxExclusive = int.MaxValue)
        {
            Counter++;
            return _random.Next(maxExclusive);
        }

        public int NextInt(int minInclusive, int maxExclusive)
        {
            if (minInclusive >= maxExclusive)
            {
                throw new System.ArgumentException("minInclusive must be less than maxExclusive.");
            }
            Counter++;
            return _random.Next(minInclusive, maxExclusive);
        }

        public float NextFloat(float max = 1.0f)
        {
            return NextFloat(0f, max);
        }

        public float NextFloat(float min, float max)
        {
            if (min > max)
            {
                throw new System.ArgumentException("min must be less than max.");
            }
            Counter++;
            return (float)_random.NextDouble() * (max - min) + min;
        }

        public double NextDouble()
        {
            Counter++;
            return _random.NextDouble();
        }

        public double NextDouble(double min, double max)
        {
            if (min > max)
            {
                throw new System.ArgumentException("min must be less than max.");
            }
            Counter++;
            return _random.NextDouble() * (max - min) + min;
        }

        public float NextGaussianFloat(float mean = 0f, float stdDev = 1f, float min = 0f, float max = 1f)
        {
            return (float)NextGaussianDouble(mean, stdDev, min, max);
        }

        public double NextGaussianDouble(double mean = 0.0, double stdDev = 1.0, double min = 0.0, double max = 1.0)
        {
            if (stdDev <= 0)
            {
                throw new System.ArgumentException("Standard deviation must be positive.");
            }
            if (min > max)
            {
                throw new System.ArgumentException("min must be less than max.");
            }

            // TODO:
            return mean;
        }

        public T? NextItem<T>(IEnumerable<T> items)
        {
            int num = items.Count();
            if (num == 0)
            {
                return default(T);
            }
            int index = NextInt(num);
            return items.ElementAt(index);
        }

        public T? WeightedNextItem<T>(IEnumerable<T> items, Func<T?, float> weightFunc)
        {
            return WeightedNextItem(NextFloat(), items, weightFunc);
        }

        public static T WeightedNextItem<T>(float randInput, IEnumerable<T> items, Func<T?, float> weightFunc, T fallback = default(T))
        {
            float totalWeight = items.Sum(weightFunc);
            float randomValue = randInput * totalWeight;
            foreach (T item in items)
            {
                randomValue -= weightFunc(item);
                if (randomValue <= 0f)
                {
                    return item;
                }
            }
            return fallback;
        }

        public void Shuffle<T>(IList<T> list)
        {
            int n = list.Count;
            for (int i = 0; i < n - 1; i++)
            {
                int j = NextInt(i, n);
                T temp = list[i];
                list[i] = list[j];
                list[j] = temp;
            }
        }
    }
}