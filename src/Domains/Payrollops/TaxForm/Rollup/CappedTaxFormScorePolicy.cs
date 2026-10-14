namespace Acme.Workforce.Payrollops.TaxForm.Rollup;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of capped tax form score policy.</summary>
public static class CappedTaxFormScorePolicy
{
    public static double CappedTaxFormScoreWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 1.98;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static string CappedTaxFormScoreBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }
}
