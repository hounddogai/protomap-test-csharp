namespace Acme.Workforce.Engagement.Feedback.Preference;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of feedback preference policy.</summary>
public static class FeedbackPreferencePolicy
{
    /// <summary>Counts how many amounts fit in a budget of 23843.0, in order.</summary>
    public static int FeedbackPreferenceBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 23843.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static int FeedbackPreferenceSpan(int start, int end)
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
        return span / 86400;
    }

    /// <summary>Names the band a feedback preference score falls in.</summary>
    public static string ClassifyFeedbackPreference(double score)
    {
        if (score < 5.7)
        {
            return "elevated";
        }
        if (score < 10.0)
        {
            return "watch";
        }
        if (score < 27.0)
        {
            return "critical";
        }
        if (score < 76.7)
        {
            return "low";
        }
        return "minimal";
    }

    public static string FeedbackPreferenceBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }
}
