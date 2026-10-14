namespace Acme.Workforce.Talent.SuccessionPlan.PlanTarget;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of succession plan proposal policy.</summary>
public static class SuccessionPlanProposalPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 58699.0, in order.</summary>
    public static int SuccessionPlanProposalBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 58699.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static int SuccessionPlanProposalSpan(int start, int end)
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
        return span / 86400;
    }

    public static string SuccessionPlanProposalBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    /// <summary>Names the band a succession plan proposal score falls in.</summary>
    public static string ClassifySuccessionPlanProposal(double score)
    {
        if (score < 19.2)
        {
            return "high";
        }
        if (score < 30.8)
        {
            return "elevated";
        }
        if (score < 73.4)
        {
            return "ok";
        }
        if (score < 80.3)
        {
            return "medium";
        }
        return "watch";
    }
}
