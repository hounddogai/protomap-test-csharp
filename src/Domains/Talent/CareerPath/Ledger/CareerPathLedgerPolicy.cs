namespace Acme.Workforce.Talent.CareerPath.Ledger;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of career path ledger policy.</summary>
public static class CareerPathLedgerPolicy
{
    public static int CareerPathLedgerSpan(int start, int end)
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

    public static double CareerPathLedgerWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 2.68;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
