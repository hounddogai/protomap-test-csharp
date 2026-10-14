namespace Acme.Workforce.Engagement.Feedback.SeasonalFeedbackReservation;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of strict feedback reservation policy.</summary>
public static class StrictFeedbackReservationPolicy
{
    /// <summary>Names the band a strict feedback reservation score falls in.</summary>
    public static string ClassifyStrictFeedbackReservation(double score)
    {
        if (score < 56.0)
        {
            return "minimal";
        }
        if (score < 77.7)
        {
            return "elevated";
        }
        return "medium";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeStrictFeedbackReservation()
    {
        var policy = Retry.DefaultPolicy("strict-feedback-reservation");
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

    public static int StrictFeedbackReservationSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 60;
    }

    public static List<string> MergeStrictFeedbackReservationLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
