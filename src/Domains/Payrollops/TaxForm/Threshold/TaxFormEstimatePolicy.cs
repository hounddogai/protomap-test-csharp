namespace Acme.Workforce.Payrollops.TaxForm.Threshold;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of tax form estimate policy.</summary>
public static class TaxFormEstimatePolicy
{
    /// <summary>Counts how many amounts fit in a budget of 92225.0, in order.</summary>
    public static int TaxFormEstimateBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 92225.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a tax form estimate score falls in.</summary>
    public static string ClassifyTaxFormEstimate(double score)
    {
        if (score < 11.2)
        {
            return "elevated";
        }
        if (score < 28.3)
        {
            return "high";
        }
        if (score < 59.7)
        {
            return "minimal";
        }
        return "medium";
    }

    public static List<string> MergeTaxFormEstimateLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Puts a tax form estimate code in its canonical TFE- form.</summary>
    public static string NormalizeTaxFormEstimateCode(string code)
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
}
