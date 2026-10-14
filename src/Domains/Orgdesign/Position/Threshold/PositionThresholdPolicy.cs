namespace Acme.Workforce.Orgdesign.Position.Threshold;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of position threshold policy.</summary>
public static class PositionThresholdPolicy
{
    /// <summary>Puts a position threshold code in its canonical PT- form.</summary>
    public static string NormalizePositionThresholdCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PT-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PT-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int PositionThresholdSpan(int start, int end)
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
        return span / 86400;
    }

    public static List<string> MergePositionThresholdLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Counts how many amounts fit in a budget of 27869.0, in order.</summary>
    public static int PositionThresholdBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 27869.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
