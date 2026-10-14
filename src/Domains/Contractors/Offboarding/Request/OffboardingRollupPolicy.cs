namespace Acme.Workforce.Contractors.Offboarding.Request;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of offboarding rollup policy.</summary>
public static class OffboardingRollupPolicy
{
    public static double OffboardingRollupWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 2.67;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static int OffboardingRollupSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 3600;
    }

    public static string OffboardingRollupBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }

    /// <summary>Puts a offboarding rollup code in its canonical OR- form.</summary>
    public static string NormalizeOffboardingRollupCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("OR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"OR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
