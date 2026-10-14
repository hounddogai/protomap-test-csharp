namespace Acme.Workforce.Talent.Nomination.Cycle;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of nomination cycle policy.</summary>
public static class NominationCyclePolicy
{
    public static int NominationCycleSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 60)
        {
            return 0;
        }
        return span / 3600;
    }

    public static List<string> MergeNominationCycleLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Puts a nomination cycle code in its canonical NC- form.</summary>
    public static string NormalizeNominationCycleCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("NC-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"NC-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
