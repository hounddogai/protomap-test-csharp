namespace Acme.Workforce.Orgdesign.CostModel.ModelEntry;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of capped cost model draft policy.</summary>
public static class CappedCostModelDraftPolicy
{
    /// <summary>Names the band a capped cost model draft score falls in.</summary>
    public static string ClassifyCappedCostModelDraft(double score)
    {
        if (score < 13.0)
        {
            return "elevated";
        }
        if (score < 30.5)
        {
            return "minimal";
        }
        if (score < 50.1)
        {
            return "low";
        }
        if (score < 52.4)
        {
            return "medium";
        }
        return "critical";
    }

    /// <summary>Counts how many amounts fit in a budget of 42104.0, in order.</summary>
    public static int CappedCostModelDraftBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 42104.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static int CappedCostModelDraftSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 900)
        {
            return 0;
        }
        return span / 3600;
    }

    public static double CappedCostModelDraftWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 0.28;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
