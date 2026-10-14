namespace Acme.Workforce.Workplace.Tests.Visitor.Slot;

using Acme.Workforce.Workplace.Visitor.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class VisitorAllocationChecks
{
    public static void VisitorAllocationStartsInPlanned()
    {
        var item = VisitorAllocation.NewVisitorAllocation("code-514", "label-597");
        Check.Equal(item.Status, VisitorAllocationStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void VisitorAllocationMovesToCompleted()
    {
        var item = VisitorAllocation.NewVisitorAllocation("code-514", "label-597");
        try
        {
            item.MoveTo(VisitorAllocationStatus.Completed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, VisitorAllocationStatus.Completed, "status");
    }

    public static void VisitorAllocationRejectsMovesFromCancelled()
    {
        var item = VisitorAllocation.NewVisitorAllocation("code-514", "label-597");
        item.Status = VisitorAllocationStatus.Cancelled;
        Check.Fails(() => item.MoveTo(VisitorAllocationStatus.Planned), "move");
    }

    public static void RunAll()
    {
        VisitorAllocationStartsInPlanned();
        VisitorAllocationMovesToCompleted();
        VisitorAllocationRejectsMovesFromCancelled();
    }
}
