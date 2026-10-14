namespace Acme.Workforce.Compensation.EquityGrant.Quota;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of equity grant proposal policy.</summary>
public static class EquityGrantProposalPolicy
{
    public static double EquityGrantProposalWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 1.18;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Counts how many amounts fit in a budget of 87328.0, in order.</summary>
    public static int EquityGrantProposalBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 87328.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a equity grant proposal score falls in.</summary>
    public static string ClassifyEquityGrantProposal(double score)
    {
        if (score < 94.7)
        {
            return "medium";
        }
        if (score < 96.4)
        {
            return "low";
        }
        return "watch";
    }
}
