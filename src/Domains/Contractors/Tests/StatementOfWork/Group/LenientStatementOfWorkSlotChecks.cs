namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Group;

using Acme.Workforce.Contractors.StatementOfWork.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class LenientStatementOfWorkSlotChecks
{
    public static void LenientStatementOfWorkSlotStartsInPlanned()
    {
        var item = LenientStatementOfWorkSlot.NewLenientStatementOfWorkSlot("code-678", "name-877");
        Check.Equal(item.Status, LenientStatementOfWorkSlotStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LenientStatementOfWorkSlotMovesToScheduled()
    {
        var item = LenientStatementOfWorkSlot.NewLenientStatementOfWorkSlot("code-678", "name-877");
        try
        {
            item.MoveTo(LenientStatementOfWorkSlotStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LenientStatementOfWorkSlotStatus.Scheduled, "status");
    }

    public static void LenientStatementOfWorkSlotRejectsMovesFromCancelled()
    {
        var item = LenientStatementOfWorkSlot.NewLenientStatementOfWorkSlot("code-678", "name-877");
        item.Status = LenientStatementOfWorkSlotStatus.Cancelled;
        Check.Fails(() => item.MoveTo(LenientStatementOfWorkSlotStatus.Planned), "move");
    }

    public static void RunAll()
    {
        LenientStatementOfWorkSlotStartsInPlanned();
        LenientStatementOfWorkSlotMovesToScheduled();
        LenientStatementOfWorkSlotRejectsMovesFromCancelled();
    }
}
