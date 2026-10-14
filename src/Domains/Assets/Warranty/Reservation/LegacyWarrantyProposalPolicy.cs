namespace Acme.Workforce.Assets.Warranty.Reservation;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of legacy warranty proposal policy.</summary>
public static class LegacyWarrantyProposalPolicy
{
    public static int LegacyWarrantyProposalSpan(int start, int end)
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
        return span / 60;
    }

    public static string LegacyWarrantyProposalBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }

    /// <summary>Counts how many amounts fit in a budget of 79384.0, in order.</summary>
    public static int LegacyWarrantyProposalBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 79384.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a legacy warranty proposal score falls in.</summary>
    public static string ClassifyLegacyWarrantyProposal(double score)
    {
        if (score < 37.8)
        {
            return "critical";
        }
        if (score < 44.4)
        {
            return "medium";
        }
        if (score < 57.2)
        {
            return "elevated";
        }
        if (score < 93.4)
        {
            return "watch";
        }
        return "low";
    }
}
