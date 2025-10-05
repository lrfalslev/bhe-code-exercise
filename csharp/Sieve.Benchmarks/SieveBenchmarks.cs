using System.Collections;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using System.Collections.Concurrent;

namespace Sieve.Benchmarks;

[MemoryDiagnoser]
public class SieveBenchmarks
{
    [Params(1000, 100_000, 1_000_000, 10_000_000, 100_000_000, 1_000_000_000)]
    public int limit;
    
    const int SEGMENT_SIZE = 100_000;

    // [Params(100_000, 500_000, 1_000_000, 5_000_000, 10_000_000, 100_000_000)]
    // public int segmentSize;

    [Benchmark(Baseline = true)]
    public List<long> Sieve()
    {
        var isPrime = new bool[limit + 1];
        Array.Fill(isPrime, true, 2, (int)limit - 1);

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
        var basePrimes = BoolArraySieve(basePrimeLimit);
        var primes = basePrimes.ToList();

        var segmentSize = Math.Min(SEGMENT_SIZE, limit);

        for (int low = segmentSize; low <= limit; low += segmentSize)
        {
            int high = Math.Min(low + segmentSize - 1, limit);
            var isPrime = new bool[high - low + 1];
            Array.Fill(isPrime, true);

            foreach (var p in basePrimes)
            {
                int start = (int)Math.Max(p * p, ((low + p - 1) / p) * p);
                for (int j = start; j <= high; j += (int)p)
                    isPrime[j - low] = false;
            }

            for (int i = low; i <= high; i++)
                if (isPrime[i - low])
                    primes.Add((long)i);
        }

        return primes;
    }

    // [Benchmark]
    // public List<long> ParallelSegmentedSieve()
    // {
    //     int basePrimeLimit = (int)Math.Sqrt(limit) + 1;
    //     var basePrimes = BoolArraySieve(basePrimeLimit);

    //     var primes = new ConcurrentBag<long>(basePrimes);

    //     segmentSize = Math.Min(segmentSize, limit);

    //     Parallel.For(0, (int)((limit - segmentSize) / segmentSize) + 1, i =>
    //     {
    //         int low = segmentSize + i * segmentSize;
    //         int high = Math.Min(low + segmentSize - 1, limit);
    //         var isPrime = new bool[high - low + 1];
    //         Array.Fill(isPrime, true);

    //         foreach (var p in basePrimes)
    //         {
    //             int start = (int)Math.Max(p * p, ((low + p - 1) / p) * p);
    //             for (int j = start; j <= high; j += (int)p)
    //                 isPrime[j - low] = false;
    //         }

    //         for (int j = low; j <= high; j++)
    //             if (isPrime[j - low])
    //                 primes.Add(j);
    //     });

    //     return primes.OrderBy(p => p).ToList(); // ensure ordered output
    // }

    private List<long> BoolArraySieve(long customLimit)
    {
        var isPrime = new bool[customLimit + 1];
        Array.Fill(isPrime, true, 2, (int)customLimit - 1);

        for (int i = 2; i * i <= customLimit; i++)
        {
            if (isPrime[i])
            {
                for (int j = i * i; j <= customLimit; j += i)
                    isPrime[j] = false;
            }
        }

        var primes = new List<long>();
        for (int i = 2; i <= customLimit; i++)
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