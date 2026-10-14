namespace Acme.Workforce.Payrollops.TaxForm.Template;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of tax form template policy.</summary>
public static class TaxFormTemplatePolicy
{
    /// <summary>Names the band a tax form template score falls in.</summary>
    public static string ClassifyTaxFormTemplate(double score)
    {
        if (score < 13.8)
        {
            return "high";
        }
        if (score < 46.8)
        {
            return "minimal";
        }
        return "ok";
    }

    public static double TaxFormTemplateWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 2.84;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
