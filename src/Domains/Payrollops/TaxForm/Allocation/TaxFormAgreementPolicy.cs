namespace Acme.Workforce.Payrollops.TaxForm.Allocation;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of tax form agreement policy.</summary>
public static class TaxFormAgreementPolicy
{
    public static int TaxFormAgreementSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 3600;
    }

    public static double TaxFormAgreementWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 0.97;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a tax form agreement score falls in.</summary>
    public static string ClassifyTaxFormAgreement(double score)
    {
        if (score < 36.3)
        {
            return "watch";
        }
        if (score < 62.0)
        {
            return "critical";
        }
        if (score < 94.0)
        {
            return "ok";
        }
        if (score < 98.3)
        {
            return "high";
        }
        return "medium";
    }

    /// <summary>Counts how many amounts fit in a budget of 6686.0, in order.</summary>
    public static int TaxFormAgreementBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 6686.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
