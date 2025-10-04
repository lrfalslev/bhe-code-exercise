namespace Sieve;

public interface ISieve
{
    long NthPrime(long n);
}

public class SieveImplementation : ISieve
{
    public long NthPrime(long n)
    {
        var upperBound = EstimateUpperBound(n);
        var primes = Sieve(upperBound);

        return primes[(int)n];
    }

    private static long EstimateUpperBound(long n)
    {
        if (n < 6) 
            return 15;

        var ln = Math.Log(n);
        var lnln = Math.Log(ln);
        return (long)(n * (ln + lnln));
    }

    private static List<long> Sieve(long limit)
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
}