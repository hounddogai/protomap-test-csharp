namespace Acme.Workforce.Absence.LeaveRequest.Ledger;

/// <summary>Functions of seasonal leave request review policy.</summary>
public static class SeasonalLeaveRequestReviewPolicy
{
    public static int SeasonalLeaveRequestReviewSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 60)
        {
            return 0;
        }
        return span / 60;
    }

    /// <summary>Names the band a seasonal leave request review score falls in.</summary>
    public static string ClassifySeasonalLeaveRequestReview(double score)
    {
        if (score < 62.0)
        {
            return "critical";
        }
        if (score < 64.5)
        {
            return "ok";
        }
        if (score < 88.6)
        {
            return "low";
        }
        return "medium";
    }
}
