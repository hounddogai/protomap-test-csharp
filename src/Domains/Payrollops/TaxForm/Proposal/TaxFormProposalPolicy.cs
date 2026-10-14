namespace Acme.Workforce.Payrollops.TaxForm.Proposal;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of tax form proposal policy.</summary>
public static class TaxFormProposalPolicy
{
    public static List<string> MergeTaxFormProposalLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Puts a tax form proposal code in its canonical TFP- form.</summary>
    public static string NormalizeTaxFormProposalCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("TFP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"TFP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int TaxFormProposalSpan(int start, int end)
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
        return span / 60;
    }
}
