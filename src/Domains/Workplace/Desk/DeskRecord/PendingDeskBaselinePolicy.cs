namespace Acme.Workforce.Workplace.Desk.DeskRecord;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of pending desk baseline policy.</summary>
public static class PendingDeskBaselinePolicy
{
    public static string PendingDeskBaselineBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }

    /// <summary>Puts a pending desk baseline code in its canonical PDB- form.</summary>
    public static string NormalizePendingDeskBaselineCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PDB-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PDB-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double PendingDeskBaselineWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 0.68;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
