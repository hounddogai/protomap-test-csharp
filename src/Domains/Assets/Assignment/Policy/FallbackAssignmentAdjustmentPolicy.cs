namespace Acme.Workforce.Assets.Assignment.Policy;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of fallback assignment adjustment policy.</summary>
public static class FallbackAssignmentAdjustmentPolicy
{
    public static List<string> MergeFallbackAssignmentAdjustmentLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Counts how many amounts fit in a budget of 1283.0, in order.</summary>
    public static int FallbackAssignmentAdjustmentBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 1283.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Puts a fallback assignment adjustment code in its canonical FAA- form.</summary>
    public static string NormalizeFallbackAssignmentAdjustmentCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("FAA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"FAA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int FallbackAssignmentAdjustmentSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 300)
        {
            return 0;
        }
        return span / 60;
    }
}
