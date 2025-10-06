using System.Collections;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using System.Collections.Concurrent;

namespace Sieve.Benchmarks;

[MemoryDiagnoser]
public class SieveBenchmarks
{
    // [Params(1000, 100_000, 1_000_000, 10_000_000, 100_000_000, 1_000_000_000)]
    [Params(1000, 100_000, 1_000_000, 100_000_000)]
    public int limit;
    
    // const int SEGMENT_SIZE = 100_000;

    [Params(100_000, 500_000, 1_000_000)]
    // [Params(100_000)]
    public int segmentSize;

    [Benchmark(Baseline = true)]
    public List<long> Sieve()
    {
        var isPrime = new bool[limit + 1];
        Array.Fill(isPrime, true, 2, limit - 1);

        for (int i = 2; i * i <= limit; i++)
        {
            if (isPrime[i])
            {
                for (int j = i * i; j <= limit; j += i)
                    isPrime[j] = false;
            }
        }

        var primes = new List<long>();
        for (int i = 2; i <= limit; i++)
            if (isPrime[i])
                primes.Add(i);

        return primes;
    }
    
    [Benchmark]
    public List<long> SegmentedSieve()
    {
        int basePrimeLimit = (int)Math.Sqrt(limit) + 1;
        var basePrimes = Sieve(basePrimeLimit);
        var primes = basePrimes.ToList();

        segmentSize = Math.Min(segmentSize, limit);

        for (int low = basePrimeLimit + 1; low <= limit; low += segmentSize)
        {
            var high = Math.Min(low + segmentSize - 1, limit);
            var isPrime = new bool[high - low + 1];
            Array.Fill(isPrime, true);

            foreach (var prime in basePrimes)
            {
                var remainder = low % prime;
                var start = remainder == 0 ? low : (int)(low + (prime - remainder));

                for (int j = start; j <= high; j += (int)prime)
                    isPrime[j - low] = false;
            }

            for (int i = low; i <= high; i++)
                if (isPrime[i - low])
                    primes.Add(i);
        }

        return primes;
    }

    [Benchmark]
    public List<long> ParallelSegmentedSieve()
    {
        int basePrimeLimit = (int)Math.Sqrt(limit) + 1;
        var basePrimes = Sieve(basePrimeLimit);
        int numSegments = (limit - basePrimeLimit) / segmentSize + 1;

        var localPrimes = new List<long>[numSegments];

        Parallel.For(0, numSegments, i =>
        {
            int low = basePrimeLimit + 1 + i * segmentSize;
            int high = Math.Min(low + segmentSize - 1, limit);
            var isPrime = new bool[high - low + 1];
            Array.Fill(isPrime, true);

            foreach (var prime in basePrimes)
            {
                var remainder = low % prime;
                var start = remainder == 0 ? low : (int)(low + (prime - remainder));
                for (long j = start; j <= high; j += prime)
                    isPrime[j - low] = false;
            }

            var segmentPrimes = new List<long>();
            for (int j = low; j <= high; j++)
                if (isPrime[j - low])
                    segmentPrimes.Add(j);

            localPrimes[i] = segmentPrimes;
        });

        return basePrimes.Concat(localPrimes.SelectMany(x => x)).ToList();
    }

    private List<long> Sieve(int limit)
    {
        var isPrime = new bool[limit + 1];
        Array.Fill(isPrime, true, 2, limit - 1);

        for (int i = 2; i * i <= limit; i++)
        {
            if (isPrime[i])
            {
                for (int j = i * i; j <= limit; j += i)
                    isPrime[j] = false;
            }
        }

        var primes = new List<long>();
        for (int i = 2; i <= limit; i++)
            if (isPrime[i])
                primes.Add(i);

        return primes;
    }
}

public class Program
{
    public static void Main(string[] args)
    {
        var summary = BenchmarkRunner.Run<SieveBenchmarks>();
    }
}