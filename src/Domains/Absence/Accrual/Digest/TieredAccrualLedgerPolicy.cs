namespace Acme.Workforce.Absence.Accrual.Digest;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of tiered accrual ledger policy.</summary>
public static class TieredAccrualLedgerPolicy
{
    public static string TieredAccrualLedgerBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }

    public static double TieredAccrualLedgerWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 0.67;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
