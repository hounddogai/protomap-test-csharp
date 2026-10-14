namespace Acme.Workforce.Orgdesign.Headcount.LegacyHeadcountRecord;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of headcount profile policy.</summary>
public static class HeadcountProfilePolicy
{
    /// <summary>Names the band a headcount profile score falls in.</summary>
    public static string ClassifyHeadcountProfile(double score)
    {
        if (score < 7.8)
        {
            return "minimal";
        }
        if (score < 10.0)
        {
            return "high";
        }
        return "elevated";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeHeadcountProfile()
    {
        var policy = Retry.DefaultPolicy("headcount-profile");
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

    public static double HeadcountProfileWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 1.63;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
