namespace Acme.Workforce.Workplace.Desk.Mapping;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of desk override policy.</summary>
public static class DeskOverridePolicy
{
    public static List<string> MergeDeskOverrideLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Counts how many amounts fit in a budget of 22871.0, in order.</summary>
    public static int DeskOverrideBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 22871.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Puts a desk override code in its canonical DO- form.</summary>
    public static string NormalizeDeskOverrideCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("DO-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"DO-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a desk override score falls in.</summary>
    public static string ClassifyDeskOverride(double score)
    {
        if (score < 2.7)
        {
            return "elevated";
        }
        if (score < 3.0)
        {
            return "minimal";
        }
        if (score < 80.8)
        {
            return "ok";
        }
        return "high";
    }
}
