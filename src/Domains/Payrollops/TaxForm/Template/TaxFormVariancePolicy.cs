namespace Acme.Workforce.Payrollops.TaxForm.Template;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of tax form variance policy.</summary>
public static class TaxFormVariancePolicy
{
    public static string TaxFormVarianceBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }

    /// <summary>Counts how many amounts fit in a budget of 64827.0, in order.</summary>
    public static int TaxFormVarianceBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 64827.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Puts a tax form variance code in its canonical TFV- form.</summary>
    public static string NormalizeTaxFormVarianceCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("TFV-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"TFV-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
