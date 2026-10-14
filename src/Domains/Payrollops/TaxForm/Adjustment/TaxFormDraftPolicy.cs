namespace Acme.Workforce.Payrollops.TaxForm.Adjustment;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of tax form draft policy.</summary>
public static class TaxFormDraftPolicy
{
    /// <summary>Puts a tax form draft code in its canonical TFD- form.</summary>
    public static string NormalizeTaxFormDraftCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("TFD-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"TFD-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static List<string> MergeTaxFormDraftLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a tax form draft score falls in.</summary>
    public static string ClassifyTaxFormDraft(double score)
    {
        if (score < 12.8)
        {
            return "low";
        }
        if (score < 35.3)
        {
            return "high";
        }
        return "medium";
    }
}
