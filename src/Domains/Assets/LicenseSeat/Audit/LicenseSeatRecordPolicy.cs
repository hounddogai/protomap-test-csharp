namespace Acme.Workforce.Assets.LicenseSeat.Audit;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of license seat record policy.</summary>
public static class LicenseSeatRecordPolicy
{
    public static string LicenseSeatRecordBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }

    public static double LicenseSeatRecordWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 2.09;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static int LicenseSeatRecordSpan(int start, int end)
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
}
