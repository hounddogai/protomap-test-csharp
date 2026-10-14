namespace Acme.Workforce.Engagement.Sentiment.Adjustment;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of sentiment adjustment policy.</summary>
public static class SentimentAdjustmentPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 54264.0, in order.</summary>
    public static int SentimentAdjustmentBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 54264.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static List<string> MergeSentimentAdjustmentLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static string SentimentAdjustmentBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    /// <summary>Names the band a sentiment adjustment score falls in.</summary>
    public static string ClassifySentimentAdjustment(double score)
    {
        if (score < 33.3)
        {
            return "minimal";
        }
        if (score < 35.8)
        {
            return "medium";
        }
        if (score < 60.0)
        {
            return "critical";
        }
        return "high";
    }
}
