namespace Acme.Workforce.Orgdesign.Position.RecordItem;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of position record policy.</summary>
public static class PositionRecordPolicy
{
    public static string PositionRecordBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    public static int PositionRecordSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 86400;
    }
}
