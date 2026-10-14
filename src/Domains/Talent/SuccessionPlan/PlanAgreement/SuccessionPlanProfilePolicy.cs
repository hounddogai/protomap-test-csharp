namespace Acme.Workforce.Talent.SuccessionPlan.PlanAgreement;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of succession plan profile policy.</summary>
public static class SuccessionPlanProfilePolicy
{
    /// <summary>Counts how many amounts fit in a budget of 12124.0, in order.</summary>
    public static int SuccessionPlanProfileBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 12124.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeSuccessionPlanProfile()
    {
        var policy = Retry.DefaultPolicy("succession-plan-profile");
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

    public static List<string> MergeSuccessionPlanProfileLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a succession plan profile score falls in.</summary>
    public static string ClassifySuccessionPlanProfile(double score)
    {
        if (score < 11.4)
        {
            return "critical";
        }
        if (score < 20.4)
        {
            return "watch";
        }
        if (score < 52.3)
        {
            return "low";
        }
        if (score < 88.6)
        {
            return "medium";
        }
        return "minimal";
    }
}
