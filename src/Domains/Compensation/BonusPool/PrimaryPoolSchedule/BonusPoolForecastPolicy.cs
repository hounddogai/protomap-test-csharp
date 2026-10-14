namespace Acme.Workforce.Compensation.BonusPool.PrimaryPoolSchedule;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of bonus pool forecast policy.</summary>
public static class BonusPoolForecastPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeBonusPoolForecast()
    {
        var policy = Retry.DefaultPolicy("bonus-pool-forecast");
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

    /// <summary>Names the band a bonus pool forecast score falls in.</summary>
    public static string ClassifyBonusPoolForecast(double score)
    {
        if (score < 67.8)
        {
            return "critical";
        }
        if (score < 79.1)
        {
            return "medium";
        }
        return "high";
    }

    public static string BonusPoolForecastBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }
}
