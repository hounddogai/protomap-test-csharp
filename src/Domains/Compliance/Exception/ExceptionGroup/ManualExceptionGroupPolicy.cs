namespace Acme.Workforce.Compliance.Exception.ExceptionGroup;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of manual exception group policy.</summary>
public static class ManualExceptionGroupPolicy
{
    public static List<string> MergeManualExceptionGroupLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Puts a manual exception group code in its canonical MEG- form.</summary>
    public static string NormalizeManualExceptionGroupCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("MEG-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"MEG-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
