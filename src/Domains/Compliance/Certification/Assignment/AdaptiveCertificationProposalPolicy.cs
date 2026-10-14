namespace Acme.Workforce.Compliance.Certification.Assignment;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of adaptive certification proposal policy.</summary>
public static class AdaptiveCertificationProposalPolicy
{
    /// <summary>Names the band a adaptive certification proposal score falls in.</summary>
    public static string ClassifyAdaptiveCertificationProposal(double score)
    {
        if (score < 3.2)
        {
            return "high";
        }
        if (score < 7.7)
        {
            return "critical";
        }
        if (score < 61.1)
        {
            return "elevated";
        }
        if (score < 70.4)
        {
            return "ok";
        }
        return "minimal";
    }

    public static string AdaptiveCertificationProposalBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    /// <summary>Counts how many amounts fit in a budget of 42938.0, in order.</summary>
    public static int AdaptiveCertificationProposalBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 42938.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
