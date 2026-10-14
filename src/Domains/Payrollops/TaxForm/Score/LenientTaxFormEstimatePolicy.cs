namespace Acme.Workforce.Payrollops.TaxForm.Score;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of lenient tax form estimate policy.</summary>
public static class LenientTaxFormEstimatePolicy
{
    /// <summary>Puts a lenient tax form estimate code in its canonical LTFE- form.</summary>
    public static string NormalizeLenientTaxFormEstimateCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LTFE-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LTFE-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static List<string> MergeLenientTaxFormEstimateLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static double LenientTaxFormEstimateWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 2.14;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Counts how many amounts fit in a budget of 95937.0, in order.</summary>
    public static int LenientTaxFormEstimateBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 95937.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
