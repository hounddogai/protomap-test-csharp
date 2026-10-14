namespace Acme.Workforce.Payrollops.TaxForm.Proposal;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of tax form notice policy.</summary>
public static class TaxFormNoticePolicy
{
    public static double TaxFormNoticeWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 2.04;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a tax form notice score falls in.</summary>
    public static string ClassifyTaxFormNotice(double score)
    {
        if (score < 31.6)
        {
            return "elevated";
        }
        if (score < 53.1)
        {
            return "medium";
        }
        return "low";
    }

    public static string TaxFormNoticeBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }
}
