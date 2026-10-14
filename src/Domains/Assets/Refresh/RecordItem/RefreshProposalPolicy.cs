namespace Acme.Workforce.Assets.Refresh.RecordItem;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of refresh proposal policy.</summary>
public static class RefreshProposalPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 67826.0, in order.</summary>
    public static int RefreshProposalBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 67826.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static string RefreshProposalBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }

    public static int RefreshProposalSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 3600)
        {
            return 0;
        }
        return span / 60;
    }

    /// <summary>Names the band a refresh proposal score falls in.</summary>
    public static string ClassifyRefreshProposal(double score)
    {
        if (score < 12.2)
        {
            return "critical";
        }
        if (score < 21.3)
        {
            return "high";
        }
        return "elevated";
    }
}
