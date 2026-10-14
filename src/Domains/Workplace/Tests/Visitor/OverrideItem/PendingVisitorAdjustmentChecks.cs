namespace Acme.Workforce.Workplace.Tests.Visitor.OverrideItem;

using Acme.Workforce.Workplace.Visitor.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class PendingVisitorAdjustmentChecks
{
    public static void PendingVisitorAdjustmentStartsInIdle()
    {
        var item = PendingVisitorAdjustment.NewPendingVisitorAdjustment("code-705", "title-869");
        Check.Equal(item.Status, PendingVisitorAdjustmentStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PendingVisitorAdjustmentMovesToStopped()
    {
        var item = PendingVisitorAdjustment.NewPendingVisitorAdjustment("code-705", "title-869");
        try
        {
            item.MoveTo(PendingVisitorAdjustmentStatus.Stopped);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PendingVisitorAdjustmentStatus.Stopped, "status");
    }

    public static void PendingVisitorAdjustmentRejectsMovesFromStopped()
    {
        var item = PendingVisitorAdjustment.NewPendingVisitorAdjustment("code-705", "title-869");
        item.Status = PendingVisitorAdjustmentStatus.Stopped;
        Check.Fails(() => item.MoveTo(PendingVisitorAdjustmentStatus.Idle), "move");
    }

    public static void RunAll()
    {
        PendingVisitorAdjustmentStartsInIdle();
        PendingVisitorAdjustmentMovesToStopped();
        PendingVisitorAdjustmentRejectsMovesFromStopped();
    }
}
