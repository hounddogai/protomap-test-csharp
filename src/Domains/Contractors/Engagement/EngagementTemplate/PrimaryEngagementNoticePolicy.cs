namespace Acme.Workforce.Contractors.Engagement.EngagementTemplate;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of primary engagement notice policy.</summary>
public static class PrimaryEngagementNoticePolicy
{
    public static string PrimaryEngagementNoticeBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }

    public static double PrimaryEngagementNoticeWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 0.84;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
