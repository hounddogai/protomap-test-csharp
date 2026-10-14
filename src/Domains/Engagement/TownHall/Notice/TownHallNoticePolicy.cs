namespace Acme.Workforce.Engagement.TownHall.Notice;

/// <summary>Functions of town hall notice policy.</summary>
public static class TownHallNoticePolicy
{
    public static int TownHallNoticeSpan(int start, int end)
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

    /// <summary>Counts how many amounts fit in a budget of 53497.0, in order.</summary>
    public static int TownHallNoticeBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 53497.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a town hall notice score falls in.</summary>
    public static string ClassifyTownHallNotice(double score)
    {
        if (score < 3.6)
        {
            return "watch";
        }
        if (score < 51.5)
        {
            return "medium";
        }
        return "high";
    }
}
