namespace Acme.Workforce.Engagement.Recognition.OverrideItem;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of recognition review policy.</summary>
public static class RecognitionReviewPolicy
{
    /// <summary>Names the band a recognition review score falls in.</summary>
    public static string ClassifyRecognitionReview(double score)
    {
        if (score < 17.9)
        {
            return "high";
        }
        if (score < 51.1)
        {
            return "low";
        }
        if (score < 54.2)
        {
            return "medium";
        }
        return "elevated";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeRecognitionReview()
    {
        var policy = Retry.DefaultPolicy("recognition-review");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 4);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    public static string RecognitionReviewBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    public static int RecognitionReviewSpan(int start, int end)
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
        return span / 60;
    }
}
