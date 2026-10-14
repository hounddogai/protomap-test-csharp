namespace Acme.Workforce.Compliance.TrainingDue.Metric;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of seasonal training due baseline policy.</summary>
public static class SeasonalTrainingDueBaselinePolicy
{
    /// <summary>Puts a seasonal training due baseline code in its canonical STDB- form.</summary>
    public static string NormalizeSeasonalTrainingDueBaselineCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("STDB-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"STDB-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static List<string> MergeSeasonalTrainingDueBaselineLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a seasonal training due baseline score falls in.</summary>
    public static string ClassifySeasonalTrainingDueBaseline(double score)
    {
        if (score < 8.1)
        {
            return "low";
        }
        if (score < 58.4)
        {
            return "medium";
        }
        return "elevated";
    }

    /// <summary>Counts how many amounts fit in a budget of 84297.0, in order.</summary>
    public static int SeasonalTrainingDueBaselineBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 84297.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
