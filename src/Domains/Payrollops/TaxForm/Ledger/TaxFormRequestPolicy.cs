namespace Acme.Workforce.Payrollops.TaxForm.Ledger;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of tax form request policy.</summary>
public static class TaxFormRequestPolicy
{
    public static double TaxFormRequestWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 0.91;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Counts how many amounts fit in a budget of 93122.0, in order.</summary>
    public static int TaxFormRequestBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 93122.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Puts a tax form request code in its canonical TFR- form.</summary>
    public static string NormalizeTaxFormRequestCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("TFR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"TFR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
