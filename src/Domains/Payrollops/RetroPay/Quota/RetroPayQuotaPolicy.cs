namespace Acme.Workforce.Payrollops.RetroPay.Quota;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of retro pay quota policy.</summary>
public static class RetroPayQuotaPolicy
{
    /// <summary>Puts a retro pay quota code in its canonical RPQ- form.</summary>
    public static string NormalizeRetroPayQuotaCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("RPQ-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"RPQ-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static List<string> MergeRetroPayQuotaLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static double RetroPayQuotaWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 1.21;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
