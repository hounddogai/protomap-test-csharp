namespace Acme.Workforce.Talent.SuccessionPlan.Variance;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of succession plan draft policy.</summary>
public static class SuccessionPlanDraftPolicy
{
    public static List<string> MergeSuccessionPlanDraftLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Puts a succession plan draft code in its canonical SPD- form.</summary>
    public static string NormalizeSuccessionPlanDraftCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SPD-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SPD-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int SuccessionPlanDraftSpan(int start, int end)
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
