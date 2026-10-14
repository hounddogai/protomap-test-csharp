namespace Acme.Workforce.Orgdesign.Span.Preference;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of lenient span ledger policy.</summary>
public static class LenientSpanLedgerPolicy
{
    /// <summary>Names the band a lenient span ledger score falls in.</summary>
    public static string ClassifyLenientSpanLedger(double score)
    {
        if (score < 13.4)
        {
            return "critical";
        }
        if (score < 56.7)
        {
            return "minimal";
        }
        if (score < 80.8)
        {
            return "elevated";
        }
        return "ok";
    }

    public static List<string> MergeLenientSpanLedgerLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
