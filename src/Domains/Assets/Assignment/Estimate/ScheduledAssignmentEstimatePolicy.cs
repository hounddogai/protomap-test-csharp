namespace Acme.Workforce.Assets.Assignment.Estimate;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of scheduled assignment estimate policy.</summary>
public static class ScheduledAssignmentEstimatePolicy
{
    public static int ScheduledAssignmentEstimateSpan(int start, int end)
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
        return span / 86400;
    }

    public static List<string> MergeScheduledAssignmentEstimateLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Counts how many amounts fit in a budget of 57288.0, in order.</summary>
    public static int ScheduledAssignmentEstimateBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 57288.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Puts a scheduled assignment estimate code in its canonical SAE- form.</summary>
    public static string NormalizeScheduledAssignmentEstimateCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SAE-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SAE-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
