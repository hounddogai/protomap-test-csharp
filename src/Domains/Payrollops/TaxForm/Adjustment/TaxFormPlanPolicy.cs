namespace Acme.Workforce.Payrollops.TaxForm.Adjustment;

/// <summary>Functions of tax form plan policy.</summary>
public static class TaxFormPlanPolicy
{
    /// <summary>Puts a tax form plan code in its canonical TFP- form.</summary>
    public static string NormalizeTaxFormPlanCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("TFP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"TFP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int TaxFormPlanSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 3600;
    }

    /// <summary>Names the band a tax form plan score falls in.</summary>
    public static string ClassifyTaxFormPlan(double score)
    {
        if (score < 13.4)
        {
            return "watch";
        }
        if (score < 73.8)
        {
            return "critical";
        }
        if (score < 78.4)
        {
            return "minimal";
        }
        if (score < 79.3)
        {
            return "medium";
        }
        return "high";
    }

    /// <summary>Counts how many amounts fit in a budget of 5178.0, in order.</summary>
    public static int TaxFormPlanBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 5178.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
