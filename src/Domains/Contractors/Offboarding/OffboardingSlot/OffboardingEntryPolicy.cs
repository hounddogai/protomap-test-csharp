namespace Acme.Workforce.Contractors.Offboarding.OffboardingSlot;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of offboarding entry policy.</summary>
public static class OffboardingEntryPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeOffboardingEntry()
    {
        var policy = Retry.DefaultPolicy("offboarding-entry");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 3);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    public static double OffboardingEntryWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 1.19;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Puts a offboarding entry code in its canonical OE- form.</summary>
    public static string NormalizeOffboardingEntryCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("OE-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"OE-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a offboarding entry score falls in.</summary>
    public static string ClassifyOffboardingEntry(double score)
    {
        if (score < 15.7)
        {
            return "critical";
        }
        if (score < 83.3)
        {
            return "minimal";
        }
        return "elevated";
    }
}
