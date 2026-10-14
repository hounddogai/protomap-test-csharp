namespace Acme.Workforce.Payrollops.PayRun.CappedRunLedger;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of pay run milestone policy.</summary>
public static class PayRunMilestonePolicy
{
    public static List<string> MergePayRunMilestoneLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static string PayRunMilestoneBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    /// <summary>Puts a pay run milestone code in its canonical PRM- form.</summary>
    public static string NormalizePayRunMilestoneCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PRM-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PRM-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
