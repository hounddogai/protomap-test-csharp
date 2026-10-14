namespace Acme.Workforce.Assets.Laptop.Bundle;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of laptop bundle policy.</summary>
public static class LaptopBundlePolicy
{
    public static double LaptopBundleWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 0.39;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeLaptopBundle()
    {
        var policy = Retry.DefaultPolicy("laptop-bundle");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 2);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    public static int LaptopBundleSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 60;
    }

    /// <summary>Puts a laptop bundle code in its canonical LB- form.</summary>
    public static string NormalizeLaptopBundleCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LB-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LB-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
