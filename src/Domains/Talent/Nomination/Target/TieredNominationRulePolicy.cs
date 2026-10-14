namespace Acme.Workforce.Talent.Nomination.Target;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of tiered nomination rule policy.</summary>
public static class TieredNominationRulePolicy
{
    public static double TieredNominationRuleWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 1.99;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a tiered nomination rule score falls in.</summary>
    public static string ClassifyTieredNominationRule(double score)
    {
        if (score < 2.1)
        {
            return "watch";
        }
        if (score < 5.3)
        {
            return "ok";
        }
        return "elevated";
    }

    public static List<string> MergeTieredNominationRuleLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeTieredNominationRule()
    {
        var policy = Retry.DefaultPolicy("tiered-nomination-rule");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 3);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }
}
