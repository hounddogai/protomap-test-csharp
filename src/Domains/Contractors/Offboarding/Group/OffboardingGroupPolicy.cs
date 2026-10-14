namespace Acme.Workforce.Contractors.Offboarding.Group;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of offboarding group policy.</summary>
public static class OffboardingGroupPolicy
{
    public static List<string> MergeOffboardingGroupLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Puts a offboarding group code in its canonical OG- form.</summary>
    public static string NormalizeOffboardingGroupCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("OG-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"OG-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Counts how many amounts fit in a budget of 89217.0, in order.</summary>
    public static int OffboardingGroupBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 89217.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a offboarding group score falls in.</summary>
    public static string ClassifyOffboardingGroup(double score)
    {
        if (score < 18.7)
        {
            return "low";
        }
        if (score < 29.2)
        {
            return "critical";
        }
        if (score < 73.7)
        {
            return "high";
        }
        return "minimal";
    }
}
