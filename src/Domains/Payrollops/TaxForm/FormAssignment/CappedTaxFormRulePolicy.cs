namespace Acme.Workforce.Payrollops.TaxForm.FormAssignment;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of capped tax form rule policy.</summary>
public static class CappedTaxFormRulePolicy
{
    public static int CappedTaxFormRuleSpan(int start, int end)
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

    /// <summary>Puts a capped tax form rule code in its canonical CTFR- form.</summary>
    public static string NormalizeCappedTaxFormRuleCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("CTFR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"CTFR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static List<string> MergeCappedTaxFormRuleLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
