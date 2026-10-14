namespace Acme.Workforce.Assets.LicenseSeat.Agreement;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of legacy license seat proposal policy.</summary>
public static class LegacyLicenseSeatProposalPolicy
{
    public static double LegacyLicenseSeatProposalWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 2.14;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a legacy license seat proposal score falls in.</summary>
    public static string ClassifyLegacyLicenseSeatProposal(double score)
    {
        if (score < 26.1)
        {
            return "medium";
        }
        if (score < 76.8)
        {
            return "elevated";
        }
        if (score < 96.7)
        {
            return "critical";
        }
        return "high";
    }

    /// <summary>Counts how many amounts fit in a budget of 93593.0, in order.</summary>
    public static int LegacyLicenseSeatProposalBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 93593.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static string LegacyLicenseSeatProposalBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }
}
