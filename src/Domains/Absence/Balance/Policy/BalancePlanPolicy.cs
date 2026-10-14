namespace Acme.Workforce.Absence.Balance.Policy;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of balance plan policy.</summary>
public static class BalancePlanPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeBalancePlan()
    {
        var policy = Retry.DefaultPolicy("balance-plan");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 5);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    public static double BalancePlanWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 1.9;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static string BalancePlanBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }

    /// <summary>Puts a balance plan code in its canonical BP- form.</summary>
    public static string NormalizeBalancePlanCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("BP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"BP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
