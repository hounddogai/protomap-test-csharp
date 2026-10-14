namespace Acme.Workforce.Absence.Balance.Target;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of balance summary policy.</summary>
public static class BalanceSummaryPolicy
{
    public static string BalanceSummaryBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    public static int BalanceSummarySpan(int start, int end)
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
        return span / 3600;
    }
}
