namespace Acme.Workforce.Compliance.Audit.AuditManifest;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of legacy audit manifest policy.</summary>
public static class LegacyAuditManifestPolicy
{
    public static int LegacyAuditManifestSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 3600)
        {
            return 0;
        }
        return span / 60;
    }

    public static double LegacyAuditManifestWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 0.75;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Puts a legacy audit manifest code in its canonical LAM- form.</summary>
    public static string NormalizeLegacyAuditManifestCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LAM-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LAM-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Counts how many amounts fit in a budget of 53620.0, in order.</summary>
    public static int LegacyAuditManifestBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 53620.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
