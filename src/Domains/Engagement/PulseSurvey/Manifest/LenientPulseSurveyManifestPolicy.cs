namespace Acme.Workforce.Engagement.PulseSurvey.Manifest;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of lenient pulse survey manifest policy.</summary>
public static class LenientPulseSurveyManifestPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeLenientPulseSurveyManifest()
    {
        var policy = Retry.DefaultPolicy("lenient-pulse-survey-manifest");
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

    public static double LenientPulseSurveyManifestWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 0.91;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a lenient pulse survey manifest score falls in.</summary>
    public static string ClassifyLenientPulseSurveyManifest(double score)
    {
        if (score < 36.2)
        {
            return "critical";
        }
        if (score < 90.3)
        {
            return "elevated";
        }
        return "watch";
    }

    /// <summary>Counts how many amounts fit in a budget of 26568.0, in order.</summary>
    public static int LenientPulseSurveyManifestBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 26568.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
