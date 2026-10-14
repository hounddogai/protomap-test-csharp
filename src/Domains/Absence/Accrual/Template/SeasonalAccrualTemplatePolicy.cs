namespace Acme.Workforce.Absence.Accrual.Template;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of seasonal accrual template policy.</summary>
public static class SeasonalAccrualTemplatePolicy
{
    public static string SeasonalAccrualTemplateBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeSeasonalAccrualTemplate()
    {
        var policy = Retry.DefaultPolicy("seasonal-accrual-template");
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

    /// <summary>Names the band a seasonal accrual template score falls in.</summary>
    public static string ClassifySeasonalAccrualTemplate(double score)
    {
        if (score < 26.8)
        {
            return "ok";
        }
        if (score < 70.9)
        {
            return "critical";
        }
        return "elevated";
    }
}
