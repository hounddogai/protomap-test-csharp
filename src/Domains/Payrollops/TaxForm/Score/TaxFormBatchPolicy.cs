namespace Acme.Workforce.Payrollops.TaxForm.Score;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of tax form batch policy.</summary>
public static class TaxFormBatchPolicy
{
    public static List<string> MergeTaxFormBatchLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Puts a tax form batch code in its canonical TFB- form.</summary>
    public static string NormalizeTaxFormBatchCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("TFB-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"TFB-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a tax form batch score falls in.</summary>
    public static string ClassifyTaxFormBatch(double score)
    {
        if (score < 52.9)
        {
            return "minimal";
        }
        if (score < 92.2)
        {
            return "elevated";
        }
        return "ok";
    }
}
