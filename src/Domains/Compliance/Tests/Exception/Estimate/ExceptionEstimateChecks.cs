namespace Acme.Workforce.Compliance.Tests.Exception.Estimate;

using Acme.Workforce.Compliance.Exception.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class ExceptionEstimateChecks
{
    public static void ExceptionEstimateStartsInIdle()
    {
        var item = ExceptionEstimate.NewExceptionEstimate("code-196", "name-877");
        Check.Equal(item.Status, ExceptionEstimateStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ExceptionEstimateMovesToWarming()
    {
        var item = ExceptionEstimate.NewExceptionEstimate("code-196", "name-877");
        try
        {
            item.MoveTo(ExceptionEstimateStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ExceptionEstimateStatus.Warming, "status");
    }

    public static void ExceptionEstimateRejectsMovesFromStopped()
    {
        var item = ExceptionEstimate.NewExceptionEstimate("code-196", "name-877");
        item.Status = ExceptionEstimateStatus.Stopped;
        Check.Fails(() => item.MoveTo(ExceptionEstimateStatus.Idle), "move");
    }

    public static void RunAll()
    {
        ExceptionEstimateStartsInIdle();
        ExceptionEstimateMovesToWarming();
        ExceptionEstimateRejectsMovesFromStopped();
    }
}
