namespace Acme.Workforce.Payrollops.PayRun.Review;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of pay run profile policy.</summary>
public static class PayRunProfilePolicy
{
    public static List<string> MergePayRunProfileLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Puts a pay run profile code in its canonical PRP- form.</summary>
    public static string NormalizePayRunProfileCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PRP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PRP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
