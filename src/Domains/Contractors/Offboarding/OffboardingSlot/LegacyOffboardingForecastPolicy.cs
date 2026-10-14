namespace Acme.Workforce.Contractors.Offboarding.OffboardingSlot;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of legacy offboarding forecast policy.</summary>
public static class LegacyOffboardingForecastPolicy
{
    public static double LegacyOffboardingForecastWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 2.82;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeLegacyOffboardingForecast()
    {
        var policy = Retry.DefaultPolicy("legacy-offboarding-forecast");
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

    /// <summary>Counts how many amounts fit in a budget of 60835.0, in order.</summary>
    public static int LegacyOffboardingForecastBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 60835.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a legacy offboarding forecast score falls in.</summary>
    public static string ClassifyLegacyOffboardingForecast(double score)
    {
        if (score < 17.6)
        {
            return "minimal";
        }
        if (score < 51.3)
        {
            return "medium";
        }
        if (score < 81.5)
        {
            return "low";
        }
        if (score < 93.0)
        {
            return "critical";
        }
        return "high";
    }
}
