namespace Acme.Workforce.Engagement.Recognition.Digest;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of recognition digest policy.</summary>
public static class RecognitionDigestPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 79427.0, in order.</summary>
    public static int RecognitionDigestBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 79427.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a recognition digest score falls in.</summary>
    public static string ClassifyRecognitionDigest(double score)
    {
        if (score < 38.7)
        {
            return "elevated";
        }
        if (score < 75.6)
        {
            return "critical";
        }
        if (score < 94.0)
        {
            return "ok";
        }
        return "high";
    }

    public static int RecognitionDigestSpan(int start, int end)
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

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeRecognitionDigest()
    {
        var policy = Retry.DefaultPolicy("recognition-digest");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 5);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }
}
