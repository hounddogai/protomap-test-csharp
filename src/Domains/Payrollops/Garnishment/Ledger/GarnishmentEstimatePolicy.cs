namespace Acme.Workforce.Payrollops.Garnishment.Ledger;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of garnishment estimate policy.</summary>
public static class GarnishmentEstimatePolicy
{
    public static List<string> MergeGarnishmentEstimateLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Puts a garnishment estimate code in its canonical GE- form.</summary>
    public static string NormalizeGarnishmentEstimateCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("GE-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"GE-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
