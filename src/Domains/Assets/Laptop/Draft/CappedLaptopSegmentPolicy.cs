namespace Acme.Workforce.Assets.Laptop.Draft;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of capped laptop segment policy.</summary>
public static class CappedLaptopSegmentPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeCappedLaptopSegment()
    {
        var policy = Retry.DefaultPolicy("capped-laptop-segment");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 1);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    public static double CappedLaptopSegmentWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 0.9;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Puts a capped laptop segment code in its canonical CLS- form.</summary>
    public static string NormalizeCappedLaptopSegmentCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("CLS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"CLS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
