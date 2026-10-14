namespace Acme.Workforce.Payrollops.TaxForm.Bundle;

/// <summary>Functions of tax form bundle policy.</summary>
public static class TaxFormBundlePolicy
{
    public static int TaxFormBundleSpan(int start, int end)
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
        return span / 60;
    }

    /// <summary>Names the band a tax form bundle score falls in.</summary>
    public static string ClassifyTaxFormBundle(double score)
    {
        if (score < 20.4)
        {
            return "elevated";
        }
        if (score < 61.0)
        {
            return "high";
        }
        return "medium";
    }
}
