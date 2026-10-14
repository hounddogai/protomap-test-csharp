namespace Acme.Workforce.Talent.Pool.PoolRule;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of pool exception policy.</summary>
public static class PoolExceptionPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 84725.0, in order.</summary>
    public static int PoolExceptionBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 84725.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static double PoolExceptionWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 0.72;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbePoolException()
    {
        var policy = Retry.DefaultPolicy("pool-exception");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 4);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    /// <summary>Puts a pool exception code in its canonical PE- form.</summary>
    public static string NormalizePoolExceptionCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PE-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PE-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
