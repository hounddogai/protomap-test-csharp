namespace Acme.Workforce.Workplace.AccessBadge.Ledger;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of regional access badge ledger policy.</summary>
public static class RegionalAccessBadgeLedgerPolicy
{
    public static int RegionalAccessBadgeLedgerSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 3600;
    }

    public static string RegionalAccessBadgeLedgerBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }
}
