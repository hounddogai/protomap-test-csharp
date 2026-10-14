namespace Acme.Workforce.Workplace.Desk.DeskAllocation;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of pending desk allocation policy.</summary>
public static class PendingDeskAllocationPolicy
{
    /// <summary>Names the band a pending desk allocation score falls in.</summary>
    public static string ClassifyPendingDeskAllocation(double score)
    {
        if (score < 11.5)
        {
            return "watch";
        }
        if (score < 26.8)
        {
            return "medium";
        }
        if (score < 54.3)
        {
            return "critical";
        }
        if (score < 90.1)
        {
            return "high";
        }
        return "ok";
    }

    public static double PendingDeskAllocationWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 0.21;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static string PendingDeskAllocationBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    /// <summary>Puts a pending desk allocation code in its canonical PDA- form.</summary>
    public static string NormalizePendingDeskAllocationCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PDA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PDA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
