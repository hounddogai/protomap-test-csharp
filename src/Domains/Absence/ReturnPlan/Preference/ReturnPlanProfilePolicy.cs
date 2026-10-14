namespace Acme.Workforce.Absence.ReturnPlan.Preference;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of return plan profile policy.</summary>
public static class ReturnPlanProfilePolicy
{
    public static int ReturnPlanProfileSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 300)
        {
            return 0;
        }
        return span / 3600;
    }

    public static string ReturnPlanProfileBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }

    /// <summary>Names the band a return plan profile score falls in.</summary>
    public static string ClassifyReturnPlanProfile(double score)
    {
        if (score < 43.7)
        {
            return "low";
        }
        if (score < 88.9)
        {
            return "watch";
        }
        return "minimal";
    }
}
