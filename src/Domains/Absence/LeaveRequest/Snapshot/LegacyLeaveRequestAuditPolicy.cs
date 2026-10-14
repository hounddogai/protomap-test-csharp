namespace Acme.Workforce.Absence.LeaveRequest.Snapshot;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of legacy leave request audit policy.</summary>
public static class LegacyLeaveRequestAuditPolicy
{
    /// <summary>Puts a legacy leave request audit code in its canonical LLRA- form.</summary>
    public static string NormalizeLegacyLeaveRequestAuditCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LLRA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LLRA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double LegacyLeaveRequestAuditWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 1.37;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
