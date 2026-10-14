namespace Acme.Workforce.Payrollops.Tests.PayRun.Request;

using Acme.Workforce.Payrollops.PayRun.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class PayRunVarianceChecks
{
    public static void PayRunVarianceStartsInIdle()
    {
        var item = PayRunVariance.NewPayRunVariance("code-849", "title-958");
        Check.Equal(item.Status, PayRunVarianceStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PayRunVarianceMovesToReady()
    {
        var item = PayRunVariance.NewPayRunVariance("code-849", "title-958");
        try
        {
            item.MoveTo(PayRunVarianceStatus.Ready);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PayRunVarianceStatus.Ready, "status");
    }

    public static void PayRunVarianceRejectsMovesFromStopped()
    {
        var item = PayRunVariance.NewPayRunVariance("code-849", "title-958");
        item.Status = PayRunVarianceStatus.Stopped;
        Check.Fails(() => item.MoveTo(PayRunVarianceStatus.Idle), "move");
    }

    public static void RunAll()
    {
        PayRunVarianceStartsInIdle();
        PayRunVarianceMovesToReady();
        PayRunVarianceRejectsMovesFromStopped();
    }
}
