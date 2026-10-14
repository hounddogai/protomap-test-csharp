namespace Acme.Workforce.Talent.Pool.Draft;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of pool draft policy.</summary>
public static class PoolDraftPolicy
{
    /// <summary>Puts a pool draft code in its canonical PD- form.</summary>
    public static string NormalizePoolDraftCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PD-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PD-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int PoolDraftSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 300)
        {
            return 0;
        }
        return span / 60;
    }

    public static double PoolDraftWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 0.66;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
