namespace Acme.Workforce.Engagement.Feedback.Rule;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of feedback assignment policy.</summary>
public static class FeedbackAssignmentPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeFeedbackAssignment()
    {
        var policy = Retry.DefaultPolicy("feedback-assignment");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 1);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    /// <summary>Names the band a feedback assignment score falls in.</summary>
    public static string ClassifyFeedbackAssignment(double score)
    {
        if (score < 34.3)
        {
            return "ok";
        }
        if (score < 44.9)
        {
            return "low";
        }
        return "elevated";
    }
}
