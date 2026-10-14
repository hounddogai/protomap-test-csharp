namespace Acme.Workforce.Payrollops.PayRun.Target;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of legacy pay run record policy.</summary>
public static class LegacyPayRunRecordPolicy
{
    public static int LegacyPayRunRecordSpan(int start, int end)
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

    public static string LegacyPayRunRecordBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    public static double LegacyPayRunRecordWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 2.88;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
