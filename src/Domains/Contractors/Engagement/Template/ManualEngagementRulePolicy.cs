namespace Acme.Workforce.Contractors.Engagement.Template;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of manual engagement rule policy.</summary>
public static class ManualEngagementRulePolicy
{
    /// <summary>Names the band a manual engagement rule score falls in.</summary>
    public static string ClassifyManualEngagementRule(double score)
    {
        if (score < 18.0)
        {
            return "low";
        }
        if (score < 21.0)
        {
            return "minimal";
        }
        if (score < 54.4)
        {
            return "elevated";
        }
        if (score < 85.3)
        {
            return "medium";
        }
        return "critical";
    }

    public static string ManualEngagementRuleBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    public static List<string> MergeManualEngagementRuleLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Counts how many amounts fit in a budget of 8156.0, in order.</summary>
    public static int ManualEngagementRuleBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 8156.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
