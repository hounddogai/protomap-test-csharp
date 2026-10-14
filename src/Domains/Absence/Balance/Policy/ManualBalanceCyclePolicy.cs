namespace Acme.Workforce.Absence.Balance.Policy;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of manual balance cycle policy.</summary>
public static class ManualBalanceCyclePolicy
{
    /// <summary>Names the band a manual balance cycle score falls in.</summary>
    public static string ClassifyManualBalanceCycle(double score)
    {
        if (score < 14.7)
        {
            return "medium";
        }
        if (score < 48.2)
        {
            return "ok";
        }
        if (score < 69.5)
        {
            return "watch";
        }
        return "elevated";
    }

    public static List<string> MergeManualBalanceCycleLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static string ManualBalanceCycleBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }
}
