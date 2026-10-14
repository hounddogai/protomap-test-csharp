namespace Acme.Workforce.Absence.Balance.Preference;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of default balance preference policy.</summary>
public static class DefaultBalancePreferencePolicy
{
    /// <summary>Names the band a default balance preference score falls in.</summary>
    public static string ClassifyDefaultBalancePreference(double score)
    {
        if (score < 16.7)
        {
            return "watch";
        }
        if (score < 50.0)
        {
            return "ok";
        }
        if (score < 61.4)
        {
            return "elevated";
        }
        return "medium";
    }

    public static List<string> MergeDefaultBalancePreferenceLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
