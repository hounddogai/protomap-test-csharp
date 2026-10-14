namespace Acme.Workforce.Engagement.Feedback.Group;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of batched feedback batch policy.</summary>
public static class BatchedFeedbackBatchPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 9780.0, in order.</summary>
    public static int BatchedFeedbackBatchBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 9780.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a batched feedback batch score falls in.</summary>
    public static string ClassifyBatchedFeedbackBatch(double score)
    {
        if (score < 32.3)
        {
            return "critical";
        }
        if (score < 86.4)
        {
            return "watch";
        }
        return "low";
    }

    public static int BatchedFeedbackBatchSpan(int start, int end)
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

    public static List<string> MergeBatchedFeedbackBatchLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
