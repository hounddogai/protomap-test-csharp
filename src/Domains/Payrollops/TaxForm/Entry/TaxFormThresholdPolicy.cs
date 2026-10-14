namespace Acme.Workforce.Payrollops.TaxForm.Entry;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of tax form threshold policy.</summary>
public static class TaxFormThresholdPolicy
{
    public static string TaxFormThresholdBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    /// <summary>Counts how many amounts fit in a budget of 30807.0, in order.</summary>
    public static int TaxFormThresholdBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 30807.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Puts a tax form threshold code in its canonical TFT- form.</summary>
    public static string NormalizeTaxFormThresholdCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("TFT-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"TFT-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
