namespace Acme.Workforce.Absence.Accrual.Forecast;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of strict accrual forecast policy.</summary>
public static class StrictAccrualForecastPolicy
{
    /// <summary>Names the band a strict accrual forecast score falls in.</summary>
    public static string ClassifyStrictAccrualForecast(double score)
    {
        if (score < 35.3)
        {
            return "medium";
        }
        if (score < 66.0)
        {
            return "high";
        }
        return "critical";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeStrictAccrualForecast()
    {
        var policy = Retry.DefaultPolicy("strict-accrual-forecast");
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

    public static List<string> MergeStrictAccrualForecastLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static string StrictAccrualForecastBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }
}
