namespace Acme.Workforce.Engagement.Sentiment.SentimentReservation;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of sentiment plan policy.</summary>
public static class SentimentPlanPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 21543.0, in order.</summary>
    public static int SentimentPlanBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 21543.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static string SentimentPlanBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    public static List<string> MergeSentimentPlanLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a sentiment plan score falls in.</summary>
    public static string ClassifySentimentPlan(double score)
    {
        if (score < 14.1)
        {
            return "critical";
        }
        if (score < 25.7)
        {
            return "high";
        }
        return "watch";
    }
}
