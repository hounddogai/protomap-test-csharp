namespace Acme.Workforce.Workplace.Desk.Allocation;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of desk allocation policy.</summary>
public static class DeskAllocationPolicy
{
    /// <summary>Puts a desk allocation code in its canonical DA- form.</summary>
    public static string NormalizeDeskAllocationCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("DA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"DA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double DeskAllocationWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 1.33;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a desk allocation score falls in.</summary>
    public static string ClassifyDeskAllocation(double score)
    {
        if (score < 46.9)
        {
            return "low";
        }
        if (score < 54.7)
        {
            return "watch";
        }
        if (score < 65.1)
        {
            return "elevated";
        }
        if (score < 80.6)
        {
            return "high";
        }
        return "ok";
    }
}
