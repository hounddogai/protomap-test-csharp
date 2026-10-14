namespace Acme.Workforce.Compensation.BonusPool.Policy;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of bonus pool audit policy.</summary>
public static class BonusPoolAuditPolicy
{
    public static string BonusPoolAuditBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }

    public static int BonusPoolAuditSpan(int start, int end)
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
