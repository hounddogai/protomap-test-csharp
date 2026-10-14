namespace Acme.Workforce.Contractors.StatementOfWork.Baseline;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of manual statement of work baseline policy.</summary>
public static class ManualStatementOfWorkBaselinePolicy
{
    /// <summary>Names the band a manual statement of work baseline score falls in.</summary>
    public static string ClassifyManualStatementOfWorkBaseline(double score)
    {
        if (score < 4.4)
        {
            return "low";
        }
        if (score < 20.2)
        {
            return "medium";
        }
        if (score < 93.3)
        {
            return "high";
        }
        return "watch";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeManualStatementOfWorkBaseline()
    {
        var policy = Retry.DefaultPolicy("manual-statement-of-work-baseline");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 4);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    public static string ManualStatementOfWorkBaselineBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }
}
