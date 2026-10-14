namespace Acme.Workforce.Payrollops.Garnishment.Profile;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of garnishment snapshot policy.</summary>
public static class GarnishmentSnapshotPolicy
{
    public static int GarnishmentSnapshotSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 86400;
    }

    public static string GarnishmentSnapshotBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    /// <summary>Puts a garnishment snapshot code in its canonical GS- form.</summary>
    public static string NormalizeGarnishmentSnapshotCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("GS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"GS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
