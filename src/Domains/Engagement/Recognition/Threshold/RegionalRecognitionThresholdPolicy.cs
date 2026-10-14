namespace Acme.Workforce.Engagement.Recognition.Threshold;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of regional recognition threshold policy.</summary>
public static class RegionalRecognitionThresholdPolicy
{
    /// <summary>Names the band a regional recognition threshold score falls in.</summary>
    public static string ClassifyRegionalRecognitionThreshold(double score)
    {
        if (score < 20.3)
        {
            return "ok";
        }
        if (score < 29.1)
        {
            return "medium";
        }
        if (score < 56.0)
        {
            return "low";
        }
        return "high";
    }

    /// <summary>Counts how many amounts fit in a budget of 47747.0, in order.</summary>
    public static int RegionalRecognitionThresholdBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 47747.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeRegionalRecognitionThreshold()
    {
        var policy = Retry.DefaultPolicy("regional-recognition-threshold");
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
