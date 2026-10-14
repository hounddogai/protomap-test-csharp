namespace Acme.Workforce.Workplace.Tests.Visitor.Cycle;

using Acme.Workforce.Workplace.Visitor.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class VisitorQuotaChecks
{
    public static void VisitorQuotaStartsInIdle()
    {
        var item = VisitorQuota.NewVisitorQuota("code-494", "label-594");
        Check.Equal(item.Status, VisitorQuotaStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void VisitorQuotaMovesToStopped()
    {
        var item = VisitorQuota.NewVisitorQuota("code-494", "label-594");
        try
        {
            item.MoveTo(VisitorQuotaStatus.Stopped);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, VisitorQuotaStatus.Stopped, "status");
    }

    public static void VisitorQuotaRejectsMovesFromStopped()
    {
        var item = VisitorQuota.NewVisitorQuota("code-494", "label-594");
        item.Status = VisitorQuotaStatus.Stopped;
        Check.Fails(() => item.MoveTo(VisitorQuotaStatus.Idle), "move");
    }

    public static void RunAll()
    {
        VisitorQuotaStartsInIdle();
        VisitorQuotaMovesToStopped();
        VisitorQuotaRejectsMovesFromStopped();
    }
}
