namespace Acme.Workforce.Payrollops.PayRun.Request;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of pay run variance policy.</summary>
public static class PayRunVariancePolicy
{
    public static double PayRunVarianceWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 2.08;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static List<string> MergePayRunVarianceLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Puts a pay run variance code in its canonical PRV- form.</summary>
    public static string NormalizePayRunVarianceCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PRV-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PRV-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
