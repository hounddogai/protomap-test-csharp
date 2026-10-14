namespace Acme.Workforce.Contractors.StatementOfWork.Profile;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of lenient statement of work request policy.</summary>
public static class LenientStatementOfWorkRequestPolicy
{
    public static string LenientStatementOfWorkRequestBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    /// <summary>Puts a lenient statement of work request code in its canonical LSOWR- form.</summary>
    public static string NormalizeLenientStatementOfWorkRequestCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LSOWR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LSOWR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static List<string> MergeLenientStatementOfWorkRequestLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
