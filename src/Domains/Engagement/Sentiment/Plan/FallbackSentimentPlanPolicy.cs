namespace Acme.Workforce.Engagement.Sentiment.Plan;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of fallback sentiment plan policy.</summary>
public static class FallbackSentimentPlanPolicy
{
    public static string FallbackSentimentPlanBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    /// <summary>Counts how many amounts fit in a budget of 58747.0, in order.</summary>
    public static int FallbackSentimentPlanBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 58747.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static List<string> MergeFallbackSentimentPlanLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a fallback sentiment plan score falls in.</summary>
    public static string ClassifyFallbackSentimentPlan(double score)
    {
        if (score < 48.9)
        {
            return "ok";
        }
        if (score < 52.2)
        {
            return "critical";
        }
        return "watch";
    }
}
