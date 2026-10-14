namespace Acme.Workforce.Engagement.PulseSurvey.RecordItem;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of regional pulse survey assignment policy.</summary>
public static class RegionalPulseSurveyAssignmentPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 95723.0, in order.</summary>
    public static int RegionalPulseSurveyAssignmentBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 95723.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static int RegionalPulseSurveyAssignmentSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 300)
        {
            return 0;
        }
        return span / 3600;
    }

    public static List<string> MergeRegionalPulseSurveyAssignmentLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Puts a regional pulse survey assignment code in its canonical RPSA- form.</summary>
    public static string NormalizeRegionalPulseSurveyAssignmentCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("RPSA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"RPSA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
