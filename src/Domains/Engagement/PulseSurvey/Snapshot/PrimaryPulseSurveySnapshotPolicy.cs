namespace Acme.Workforce.Engagement.PulseSurvey.Snapshot;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of primary pulse survey snapshot policy.</summary>
public static class PrimaryPulseSurveySnapshotPolicy
{
    public static double PrimaryPulseSurveySnapshotWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 1.81;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a primary pulse survey snapshot score falls in.</summary>
    public static string ClassifyPrimaryPulseSurveySnapshot(double score)
    {
        if (score < 34.5)
        {
            return "critical";
        }
        if (score < 50.4)
        {
            return "watch";
        }
        if (score < 50.5)
        {
            return "low";
        }
        if (score < 64.8)
        {
            return "minimal";
        }
        return "medium";
    }

    public static string PrimaryPulseSurveySnapshotBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbePrimaryPulseSurveySnapshot()
    {
        var policy = Retry.DefaultPolicy("primary-pulse-survey-snapshot");
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
}
