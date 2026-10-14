namespace Acme.Workforce.Absence.LeaveRequest.Review;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of strict leave request review policy.</summary>
public static class StrictLeaveRequestReviewPolicy
{
    public static int StrictLeaveRequestReviewSpan(int start, int end)
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

    public static double StrictLeaveRequestReviewWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 2.61;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static string StrictLeaveRequestReviewBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }
}
