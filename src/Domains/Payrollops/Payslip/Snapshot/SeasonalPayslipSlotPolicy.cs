namespace Acme.Workforce.Payrollops.Payslip.Snapshot;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of seasonal payslip slot policy.</summary>
public static class SeasonalPayslipSlotPolicy
{
    public static string SeasonalPayslipSlotBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    /// <summary>Puts a seasonal payslip slot code in its canonical SPS- form.</summary>
    public static string NormalizeSeasonalPayslipSlotCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SPS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SPS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static List<string> MergeSeasonalPayslipSlotLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeSeasonalPayslipSlot()
    {
        var policy = Retry.DefaultPolicy("seasonal-payslip-slot");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 5);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }
}
