namespace Acme.Workforce.Talent.CareerPath.Segment;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of career path preference policy.</summary>
public static class CareerPathPreferencePolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeCareerPathPreference()
    {
        var policy = Retry.DefaultPolicy("career-path-preference");
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

    public static double CareerPathPreferenceWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 2.62;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Counts how many amounts fit in a budget of 30148.0, in order.</summary>
    public static int CareerPathPreferenceBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 30148.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a career path preference score falls in.</summary>
    public static string ClassifyCareerPathPreference(double score)
    {
        if (score < 84.1)
        {
            return "low";
        }
        if (score < 93.9)
        {
            return "medium";
        }
        return "elevated";
    }
}
