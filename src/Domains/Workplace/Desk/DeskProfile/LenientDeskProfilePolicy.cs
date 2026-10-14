namespace Acme.Workforce.Workplace.Desk.DeskProfile;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of lenient desk profile policy.</summary>
public static class LenientDeskProfilePolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeLenientDeskProfile()
    {
        var policy = Retry.DefaultPolicy("lenient-desk-profile");
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

    /// <summary>Names the band a lenient desk profile score falls in.</summary>
    public static string ClassifyLenientDeskProfile(double score)
    {
        if (score < 5.6)
        {
            return "elevated";
        }
        if (score < 13.0)
        {
            return "medium";
        }
        if (score < 66.6)
        {
            return "minimal";
        }
        return "low";
    }

    public static double LenientDeskProfileWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 1.93;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
