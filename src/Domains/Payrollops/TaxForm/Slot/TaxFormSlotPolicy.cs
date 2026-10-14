namespace Acme.Workforce.Payrollops.TaxForm.Slot;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of tax form slot policy.</summary>
public static class TaxFormSlotPolicy
{
    /// <summary>Puts a tax form slot code in its canonical TFS- form.</summary>
    public static string NormalizeTaxFormSlotCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("TFS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"TFS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int TaxFormSlotSpan(int start, int end)
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

    /// <summary>Counts how many amounts fit in a budget of 3603.0, in order.</summary>
    public static int TaxFormSlotBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 3603.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static double TaxFormSlotWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 2.0;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
