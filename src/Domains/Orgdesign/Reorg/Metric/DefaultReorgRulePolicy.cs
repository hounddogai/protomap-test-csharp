namespace Acme.Workforce.Orgdesign.Reorg.Metric;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of default reorg rule policy.</summary>
public static class DefaultReorgRulePolicy
{
    /// <summary>Counts how many amounts fit in a budget of 25246.0, in order.</summary>
    public static int DefaultReorgRuleBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 25246.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static List<string> MergeDefaultReorgRuleLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeDefaultReorgRule()
    {
        var policy = Retry.DefaultPolicy("default-reorg-rule");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 5);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    /// <summary>Names the band a default reorg rule score falls in.</summary>
    public static string ClassifyDefaultReorgRule(double score)
    {
        if (score < 1.5)
        {
            return "medium";
        }
        if (score < 79.7)
        {
            return "high";
        }
        return "watch";
    }
}
