namespace Acme.Workforce.Engagement.TownHall.Ledger;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of adaptive town hall notice policy.</summary>
public static class AdaptiveTownHallNoticePolicy
{
    public static string AdaptiveTownHallNoticeBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }

    public static double AdaptiveTownHallNoticeWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 2.44;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
