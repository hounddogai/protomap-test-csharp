namespace Acme.Workforce.Payrollops.PayRun.Cycle;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of legacy pay run assignment policy.</summary>
public static class LegacyPayRunAssignmentPolicy
{
    /// <summary>Puts a legacy pay run assignment code in its canonical LPRA- form.</summary>
    public static string NormalizeLegacyPayRunAssignmentCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LPRA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LPRA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string LegacyPayRunAssignmentBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    public static int LegacyPayRunAssignmentSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 900)
        {
            return 0;
        }
        return span / 86400;
    }
}
