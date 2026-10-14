namespace Acme.Workforce.Workplace.Visitor.Summary;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of manual visitor summary policy.</summary>
public static class ManualVisitorSummaryPolicy
{
    /// <summary>Names the band a manual visitor summary score falls in.</summary>
    public static string ClassifyManualVisitorSummary(double score)
    {
        if (score < 12.6)
        {
            return "minimal";
        }
        if (score < 26.5)
        {
            return "medium";
        }
        return "elevated";
    }

    public static double ManualVisitorSummaryWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 1.73;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static List<string> MergeManualVisitorSummaryLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
