namespace Acme.Workforce.Engagement.PulseSurvey.Entry;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of manual pulse survey summary policy.</summary>
public static class ManualPulseSurveySummaryPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 70629.0, in order.</summary>
    public static int ManualPulseSurveySummaryBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 70629.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeManualPulseSurveySummary()
    {
        var policy = Retry.DefaultPolicy("manual-pulse-survey-summary");
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

    public static string ManualPulseSurveySummaryBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }
}
