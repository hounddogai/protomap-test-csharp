namespace Acme.Workforce.Orgdesign.CostModel.AdaptiveModelPolicy;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of cost model draft policy.</summary>
public static class CostModelDraftPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 13241.0, in order.</summary>
    public static int CostModelDraftBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 13241.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static double CostModelDraftWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 0.93;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a cost model draft score falls in.</summary>
    public static string ClassifyCostModelDraft(double score)
    {
        if (score < 29.2)
        {
            return "high";
        }
        if (score < 44.6)
        {
            return "critical";
        }
        if (score < 74.7)
        {
            return "minimal";
        }
        return "elevated";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeCostModelDraft()
    {
        var policy = Retry.DefaultPolicy("cost-model-draft");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 1);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }
}
