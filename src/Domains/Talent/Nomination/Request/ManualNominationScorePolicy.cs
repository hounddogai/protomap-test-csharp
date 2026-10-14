namespace Acme.Workforce.Talent.Nomination.Request;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of manual nomination score policy.</summary>
public static class ManualNominationScorePolicy
{
    public static double ManualNominationScoreWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 1.56;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static List<string> MergeManualNominationScoreLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Puts a manual nomination score code in its canonical MNS- form.</summary>
    public static string NormalizeManualNominationScoreCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("MNS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"MNS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
