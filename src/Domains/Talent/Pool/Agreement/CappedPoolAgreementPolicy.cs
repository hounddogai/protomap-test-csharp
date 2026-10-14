namespace Acme.Workforce.Talent.Pool.Agreement;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of capped pool agreement policy.</summary>
public static class CappedPoolAgreementPolicy
{
    public static double CappedPoolAgreementWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 1.93;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static string CappedPoolAgreementBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }
}
