namespace Acme.Workforce.Payrollops.TaxForm.Tier;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of tax form exception policy.</summary>
public static class TaxFormExceptionPolicy
{
    public static int TaxFormExceptionSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 3600)
        {
            return 0;
        }
        return span / 86400;
    }

    /// <summary>Puts a tax form exception code in its canonical TFE- form.</summary>
    public static string NormalizeTaxFormExceptionCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("TFE-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"TFE-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string TaxFormExceptionBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    /// <summary>Counts how many amounts fit in a budget of 22515.0, in order.</summary>
    public static int TaxFormExceptionBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 22515.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
