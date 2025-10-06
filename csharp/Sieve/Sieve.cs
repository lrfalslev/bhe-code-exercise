namespace Sieve;

public interface ISieve
{
    long NthPrime(long n);
}

public class SieveImplementation : ISieve
{
    const int SEGMENT_SIZE = 1_000_000;

    public long NthPrime(long n)
    {
        if (n < 0)
            throw new ArgumentOutOfRangeException(nameof(n), "n must be non-negative.");

        var upperBound = EstimateUpperBound(n);
        var primes = ParallelSieve(upperBound, (int)n);

        return primes[(int)n];
    }

    private static int EstimateUpperBound(long n)
    {
        if (n < 6) 
            return 15;

        var ln = Math.Log(n);
        var lnln = Math.Log(ln);
        return (int)(n * (ln + lnln));
    }

    private static List<long> ParallelSieve(int limit, int count)
    {
        var primes = new List<long>(count);

        var basePrimeLimit = (int)Math.Sqrt(limit) + 1;
        var basePrimes = Sieve(basePrimeLimit);

        var segmentSize = Math.Min(SEGMENT_SIZE, limit);
        var numSegments = (limit - basePrimeLimit) / segmentSize + 1;

        var localPrimes = new List<long>[numSegments];

        Parallel.For(0, numSegments, i =>
        {
            var low = basePrimeLimit + 1 + i * segmentSize;
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

            var segmentPrimes = new List<long>();
            for (int j = low; j <= high; j++)
                if (isPrime[j - low])
                    segmentPrimes.Add(j);

            localPrimes[i] = segmentPrimes;
        });

        primes.AddRange(basePrimes);
        foreach (var segment in localPrimes)
            primes.AddRange(segment);

        return primes;
    }
    
    private static List<long> Sieve(int limit)
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