namespace Acme.Workforce.Payrollops.RetroPay.PayPolicy;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of batched retro pay plan policy.</summary>
public static class BatchedRetroPayPlanPolicy
{
    public static List<string> MergeBatchedRetroPayPlanLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Puts a batched retro pay plan code in its canonical BRPP- form.</summary>
    public static string NormalizeBatchedRetroPayPlanCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("BRPP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"BRPP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
