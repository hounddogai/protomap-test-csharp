namespace Acme.Workforce.Talent.Mobility.RecordItem;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of rolling mobility proposal policy.</summary>
public static class RollingMobilityProposalPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 21307.0, in order.</summary>
    public static int RollingMobilityProposalBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 21307.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static double RollingMobilityProposalWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 2.34;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
