namespace Acme.Workforce.Contractors.Timesheet.Exception;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of primary timesheet exception policy.</summary>
public static class PrimaryTimesheetExceptionPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbePrimaryTimesheetException()
    {
        var policy = Retry.DefaultPolicy("primary-timesheet-exception");
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

    /// <summary>Names the band a primary timesheet exception score falls in.</summary>
    public static string ClassifyPrimaryTimesheetException(double score)
    {
        if (score < 14.0)
        {
            return "elevated";
        }
        if (score < 30.4)
        {
            return "watch";
        }
        if (score < 78.0)
        {
            return "critical";
        }
        return "low";
    }
}
