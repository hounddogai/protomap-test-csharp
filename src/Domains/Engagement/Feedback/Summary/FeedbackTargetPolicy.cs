namespace Acme.Workforce.Engagement.Feedback.Summary;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of feedback target policy.</summary>
public static class FeedbackTargetPolicy
{
    /// <summary>Names the band a feedback target score falls in.</summary>
    public static string ClassifyFeedbackTarget(double score)
    {
        if (score < 12.6)
        {
            return "high";
        }
        if (score < 18.8)
        {
            return "ok";
        }
        if (score < 23.1)
        {
            return "watch";
        }
        return "minimal";
    }

    public static List<string> MergeFeedbackTargetLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Counts how many amounts fit in a budget of 58641.0, in order.</summary>
    public static int FeedbackTargetBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 58641.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static double FeedbackTargetWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 1.51;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
