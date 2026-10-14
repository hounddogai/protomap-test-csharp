namespace Acme.Workforce.Contractors.Engagement.EngagementAssignment;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of primary engagement profile policy.</summary>
public static class PrimaryEngagementProfilePolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbePrimaryEngagementProfile()
    {
        var policy = Retry.DefaultPolicy("primary-engagement-profile");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 3);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    public static double PrimaryEngagementProfileWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 2.27;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a primary engagement profile score falls in.</summary>
    public static string ClassifyPrimaryEngagementProfile(double score)
    {
        if (score < 6.5)
        {
            return "ok";
        }
        if (score < 11.8)
        {
            return "medium";
        }
        if (score < 42.1)
        {
            return "high";
        }
        if (score < 91.3)
        {
            return "critical";
        }
        return "elevated";
    }
}
