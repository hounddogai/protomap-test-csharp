namespace Acme.Workforce.Payrollops.TaxForm.FormMetric;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of tax form claim policy.</summary>
public static class TaxFormClaimPolicy
{
    public static double TaxFormClaimWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 0.11;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static List<string> MergeTaxFormClaimLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a tax form claim score falls in.</summary>
    public static string ClassifyTaxFormClaim(double score)
    {
        if (score < 9.2)
        {
            return "low";
        }
        if (score < 20.0)
        {
            return "critical";
        }
        if (score < 53.0)
        {
            return "ok";
        }
        return "minimal";
    }
}
