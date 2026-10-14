namespace Acme.Workforce.Assets.Assignment.Proposal;

/// <summary>Functions of seasonal assignment proposal policy.</summary>
public static class SeasonalAssignmentProposalPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 59658.0, in order.</summary>
    public static int SeasonalAssignmentProposalBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 59658.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static int SeasonalAssignmentProposalSpan(int start, int end)
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
}
