namespace Acme.Workforce.Compensation.EquityGrant.Ledger;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of manual equity grant draft policy.</summary>
public static class ManualEquityGrantDraftPolicy
{
    public static string ManualEquityGrantDraftBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }

    public static double ManualEquityGrantDraftWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 2.63;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static int ManualEquityGrantDraftSpan(int start, int end)
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
        return span / 86400;
    }

    /// <summary>Puts a manual equity grant draft code in its canonical MEGD- form.</summary>
    public static string NormalizeManualEquityGrantDraftCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("MEGD-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"MEGD-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
