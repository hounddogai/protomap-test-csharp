namespace Acme.Workforce.Talent.SuccessionPlan.PlanAgreement;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of lenient succession plan agreement policy.</summary>
public static class LenientSuccessionPlanAgreementPolicy
{
    /// <summary>Puts a lenient succession plan agreement code in its canonical LSPA- form.</summary>
    public static string NormalizeLenientSuccessionPlanAgreementCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LSPA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LSPA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static List<string> MergeLenientSuccessionPlanAgreementLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static int LenientSuccessionPlanAgreementSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 900)
        {
            return 0;
        }
        return span / 86400;
    }
}
