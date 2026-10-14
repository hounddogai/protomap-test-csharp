namespace Acme.Workforce.Engagement.Feedback.FeedbackProposal;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of scheduled feedback forecast policy.</summary>
public static class ScheduledFeedbackForecastPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 83100.0, in order.</summary>
    public static int ScheduledFeedbackForecastBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 83100.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Puts a scheduled feedback forecast code in its canonical SFF- form.</summary>
    public static string NormalizeScheduledFeedbackForecastCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SFF-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SFF-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static List<string> MergeScheduledFeedbackForecastLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static int ScheduledFeedbackForecastSpan(int start, int end)
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
        return span / 86400;
    }
}
