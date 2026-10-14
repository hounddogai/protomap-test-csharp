namespace Acme.Workforce.Workplace.Desk.DeskProposal;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of desk exception policy.</summary>
public static class DeskExceptionPolicy
{
    /// <summary>Puts a desk exception code in its canonical DE- form.</summary>
    public static string NormalizeDeskExceptionCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("DE-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"DE-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static List<string> MergeDeskExceptionLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
