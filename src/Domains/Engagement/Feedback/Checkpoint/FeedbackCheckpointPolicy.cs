namespace Acme.Workforce.Engagement.Feedback.Checkpoint;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of feedback checkpoint policy.</summary>
public static class FeedbackCheckpointPolicy
{
    /// <summary>Names the band a feedback checkpoint score falls in.</summary>
    public static string ClassifyFeedbackCheckpoint(double score)
    {
        if (score < 14.8)
        {
            return "low";
        }
        if (score < 28.9)
        {
            return "watch";
        }
        if (score < 29.5)
        {
            return "minimal";
        }
        return "elevated";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeFeedbackCheckpoint()
    {
        var policy = Retry.DefaultPolicy("feedback-checkpoint");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 2);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    public static List<string> MergeFeedbackCheckpointLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static int FeedbackCheckpointSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 900)
        {
            return 0;
        }
        return span / 60;
    }
}
