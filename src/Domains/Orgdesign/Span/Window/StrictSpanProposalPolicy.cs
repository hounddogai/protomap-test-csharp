namespace Acme.Workforce.Orgdesign.Span.Window;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of strict span proposal policy.</summary>
public static class StrictSpanProposalPolicy
{
    public static double StrictSpanProposalWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 2.83;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Counts how many amounts fit in a budget of 99732.0, in order.</summary>
    public static int StrictSpanProposalBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 99732.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
