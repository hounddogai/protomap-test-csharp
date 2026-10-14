namespace Acme.Workforce.Talent.SuccessionPlan.Policy;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of regional succession plan agreement policy.</summary>
public static class RegionalSuccessionPlanAgreementPolicy
{
    public static double RegionalSuccessionPlanAgreementWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 1.25;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Counts how many amounts fit in a budget of 71118.0, in order.</summary>
    public static int RegionalSuccessionPlanAgreementBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 71118.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a regional succession plan agreement score falls in.</summary>
    public static string ClassifyRegionalSuccessionPlanAgreement(double score)
    {
        if (score < 57.6)
        {
            return "low";
        }
        if (score < 68.3)
        {
            return "high";
        }
        return "medium";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeRegionalSuccessionPlanAgreement()
    {
        var policy = Retry.DefaultPolicy("regional-succession-plan-agreement");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 4);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }
}
