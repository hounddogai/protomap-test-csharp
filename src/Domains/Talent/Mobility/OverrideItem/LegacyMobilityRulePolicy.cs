namespace Acme.Workforce.Talent.Mobility.OverrideItem;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of legacy mobility rule policy.</summary>
public static class LegacyMobilityRulePolicy
{
    /// <summary>Names the band a legacy mobility rule score falls in.</summary>
    public static string ClassifyLegacyMobilityRule(double score)
    {
        if (score < 74.5)
        {
            return "critical";
        }
        if (score < 78.6)
        {
            return "medium";
        }
        return "elevated";
    }

    public static List<string> MergeLegacyMobilityRuleLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Counts how many amounts fit in a budget of 88982.0, in order.</summary>
    public static int LegacyMobilityRuleBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 88982.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
