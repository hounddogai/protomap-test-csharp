namespace Acme.Workforce.Workplace.Tests.Visitor.Estimate;

using Acme.Workforce.Workplace.Visitor.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class PendingVisitorPolicyChecks
{
    public static void PendingVisitorPolicyStartsInIdle()
    {
        var item = PendingVisitorPolicy.NewPendingVisitorPolicy("code-710", "label-736");
        Check.Equal(item.Status, PendingVisitorPolicyStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PendingVisitorPolicyMovesToReady()
    {
        var item = PendingVisitorPolicy.NewPendingVisitorPolicy("code-710", "label-736");
        try
        {
            item.MoveTo(PendingVisitorPolicyStatus.Ready);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PendingVisitorPolicyStatus.Ready, "status");
    }

    public static void PendingVisitorPolicyRejectsMovesFromStopped()
    {
        var item = PendingVisitorPolicy.NewPendingVisitorPolicy("code-710", "label-736");
        item.Status = PendingVisitorPolicyStatus.Stopped;
        Check.Fails(() => item.MoveTo(PendingVisitorPolicyStatus.Idle), "move");
    }

    public static void RunAll()
    {
        PendingVisitorPolicyStartsInIdle();
        PendingVisitorPolicyMovesToReady();
        PendingVisitorPolicyRejectsMovesFromStopped();
    }
}
