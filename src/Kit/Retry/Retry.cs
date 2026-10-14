namespace Acme.Workforce.Kit.Retry;

using System.Security.Cryptography;
using System.Text;

/// <summary>Backoff says how many milliseconds to wait before the next attempt.</summary>
public interface IBackoff
{
    int Delay(int attempt);
}

public sealed record ConstantBackoff(int Millis) : IBackoff
{
    public int Delay(int attempt)
    {
        return Millis;
    }
}

public sealed record LinearBackoff(int Step, int Cap) : IBackoff
{
    public int Delay(int attempt)
    {
        return Math.Min(Step * (attempt + 1), Cap);
    }
}

public sealed record ExponentialBackoff(int Base, double Factor, int Cap) : IBackoff
{
    public int Delay(int attempt)
    {
        var delay = (double)Base;
        for (var i = 0; i < attempt; i++)
        {
            delay = delay * Factor;
        }
        return Math.Min((int)delay, Cap);
    }
}

/// <summary>JitterBackoff spreads another backoff's delays by a stable pseudo-random amount.</summary>
public sealed record JitterBackoff(IBackoff Inner, int Spread, string Salt) : IBackoff
{
    public int Delay(int attempt)
    {
        var baseValue = Inner.Delay(attempt);
        if (Spread <= 0)
        {
            return baseValue;
        }
        var noise = (int)(BitConverter.ToUInt32(SHA256.HashData(Encoding.UTF8.GetBytes($"{Salt}:{attempt}"))) & 0x7fffffff) % Spread;
        return baseValue + noise;
    }
}

/// <summary>Policy retries an operation up to attempts times.</summary>
public sealed record Policy(int Attempts, IBackoff Backoff)
{
    /// <summary>Calls op until it succeeds and returns how many attempts it took.</summary>
    public int Run(Func<int, bool> op)
    {
        var waited = 0;
        for (var attempt = 0; attempt < Attempts; attempt++)
        {
            if (op(attempt))
            {
                return attempt + 1;
            }
            waited += Backoff.Delay(attempt);
        }
        throw new InvalidOperationException($"gave up after {Attempts} attempts and {waited} ms");
    }

    public int TotalDelay()
    {
        var total = 0;
        for (var attempt = 0; attempt < Attempts; attempt++)
        {
            total += Backoff.Delay(attempt);
        }
        return total;
    }
}

/// <summary>Functions of retry.</summary>
public static class Retry
{
    public static Policy DefaultPolicy(string salt)
    {
        return new Policy(5, new JitterBackoff(new ExponentialBackoff(100, 2.0, 5000), 50, salt));
    }
}
