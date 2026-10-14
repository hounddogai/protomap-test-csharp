namespace Acme.Workforce.Talent.Mobility.Variance;

/// <summary>Functions of primary mobility proposal policy.</summary>
public static class PrimaryMobilityProposalPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 17845.0, in order.</summary>
    public static int PrimaryMobilityProposalBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 17845.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a primary mobility proposal score falls in.</summary>
    public static string ClassifyPrimaryMobilityProposal(double score)
    {
        if (score < 3.4)
        {
            return "critical";
        }
        if (score < 17.3)
        {
            return "minimal";
        }
        if (score < 40.4)
        {
            return "medium";
        }
        if (score < 45.8)
        {
            return "elevated";
        }
        return "high";
    }
}
