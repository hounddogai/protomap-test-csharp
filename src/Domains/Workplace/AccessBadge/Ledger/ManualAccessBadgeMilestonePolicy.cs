namespace Acme.Workforce.Workplace.AccessBadge.Ledger;

/// <summary>Functions of manual access badge milestone policy.</summary>
public static class ManualAccessBadgeMilestonePolicy
{
    /// <summary>Counts how many amounts fit in a budget of 73613.0, in order.</summary>
    public static int ManualAccessBadgeMilestoneBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 73613.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a manual access badge milestone score falls in.</summary>
    public static string ClassifyManualAccessBadgeMilestone(double score)
    {
        if (score < 10.2)
        {
            return "high";
        }
        if (score < 18.5)
        {
            return "elevated";
        }
        return "ok";
    }

    public static int ManualAccessBadgeMilestoneSpan(int start, int end)
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
}
