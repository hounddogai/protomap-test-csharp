namespace Acme.Workforce.Payrollops.TaxForm.Estimate;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of lenient tax form notice policy.</summary>
public static class LenientTaxFormNoticePolicy
{
    /// <summary>Names the band a lenient tax form notice score falls in.</summary>
    public static string ClassifyLenientTaxFormNotice(double score)
    {
        if (score < 31.5)
        {
            return "watch";
        }
        if (score < 38.5)
        {
            return "ok";
        }
        return "high";
    }

    /// <summary>Counts how many amounts fit in a budget of 34709.0, in order.</summary>
    public static int LenientTaxFormNoticeBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 34709.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static double LenientTaxFormNoticeWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 1.82;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
