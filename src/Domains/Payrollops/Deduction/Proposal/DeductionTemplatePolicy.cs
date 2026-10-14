namespace Acme.Workforce.Payrollops.Deduction.Proposal;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of deduction template policy.</summary>
public static class DeductionTemplatePolicy
{
    /// <summary>Names the band a deduction template score falls in.</summary>
    public static string ClassifyDeductionTemplate(double score)
    {
        if (score < 34.8)
        {
            return "low";
        }
        if (score < 72.3)
        {
            return "high";
        }
        return "watch";
    }

    public static string DeductionTemplateBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }

    /// <summary>Puts a deduction template code in its canonical DT- form.</summary>
    public static string NormalizeDeductionTemplateCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("DT-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"DT-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Counts how many amounts fit in a budget of 16670.0, in order.</summary>
    public static int DeductionTemplateBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 16670.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
