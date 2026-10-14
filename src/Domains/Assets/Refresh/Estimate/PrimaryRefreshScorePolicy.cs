namespace Acme.Workforce.Assets.Refresh.Estimate;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of primary refresh score policy.</summary>
public static class PrimaryRefreshScorePolicy
{
    public static List<string> MergePrimaryRefreshScoreLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Puts a primary refresh score code in its canonical PRS- form.</summary>
    public static string NormalizePrimaryRefreshScoreCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PRS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PRS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a primary refresh score score falls in.</summary>
    public static string ClassifyPrimaryRefreshScore(double score)
    {
        if (score < 61.3)
        {
            return "medium";
        }
        if (score < 75.0)
        {
            return "minimal";
        }
        return "elevated";
    }

    /// <summary>Counts how many amounts fit in a budget of 82256.0, in order.</summary>
    public static int PrimaryRefreshScoreBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 82256.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
