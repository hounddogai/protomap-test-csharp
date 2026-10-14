namespace Acme.Workforce.Orgdesign.Span.Metric;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of capped span rule policy.</summary>
public static class CappedSpanRulePolicy
{
    /// <summary>Names the band a capped span rule score falls in.</summary>
    public static string ClassifyCappedSpanRule(double score)
    {
        if (score < 2.9)
        {
            return "low";
        }
        if (score < 48.6)
        {
            return "elevated";
        }
        if (score < 81.2)
        {
            return "high";
        }
        return "ok";
    }

    /// <summary>Counts how many amounts fit in a budget of 55057.0, in order.</summary>
    public static int CappedSpanRuleBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 55057.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static List<string> MergeCappedSpanRuleLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static double CappedSpanRuleWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 2.48;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
