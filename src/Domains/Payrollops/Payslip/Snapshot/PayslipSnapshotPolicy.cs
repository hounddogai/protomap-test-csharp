namespace Acme.Workforce.Payrollops.Payslip.Snapshot;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of payslip snapshot policy.</summary>
public static class PayslipSnapshotPolicy
{
    /// <summary>Puts a payslip snapshot code in its canonical PS- form.</summary>
    public static string NormalizePayslipSnapshotCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double PayslipSnapshotWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 2.17;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static string PayslipSnapshotBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }
}
