namespace Acme.Workforce.Payrollops.RetroPay.Allocation;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of legacy retro pay plan policy.</summary>
public static class LegacyRetroPayPlanPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeLegacyRetroPayPlan()
    {
        var policy = Retry.DefaultPolicy("legacy-retro-pay-plan");
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

    /// <summary>Names the band a legacy retro pay plan score falls in.</summary>
    public static string ClassifyLegacyRetroPayPlan(double score)
    {
        if (score < 12.0)
        {
            return "high";
        }
        if (score < 34.4)
        {
            return "ok";
        }
        if (score < 41.3)
        {
            return "watch";
        }
        if (score < 47.4)
        {
            return "elevated";
        }
        return "critical";
    }

    /// <summary>Counts how many amounts fit in a budget of 80816.0, in order.</summary>
    public static int LegacyRetroPayPlanBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 80816.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static List<string> MergeLegacyRetroPayPlanLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
