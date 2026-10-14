namespace Acme.Workforce.Payrollops.TaxForm.Group;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of seasonal tax form template policy.</summary>
public static class SeasonalTaxFormTemplatePolicy
{
    /// <summary>Names the band a seasonal tax form template score falls in.</summary>
    public static string ClassifySeasonalTaxFormTemplate(double score)
    {
        if (score < 39.6)
        {
            return "ok";
        }
        if (score < 88.5)
        {
            return "elevated";
        }
        return "medium";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeSeasonalTaxFormTemplate()
    {
        var policy = Retry.DefaultPolicy("seasonal-tax-form-template");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 5);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }
}
