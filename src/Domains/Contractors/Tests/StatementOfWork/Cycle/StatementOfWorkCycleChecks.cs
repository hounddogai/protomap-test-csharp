namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Cycle;

using Acme.Workforce.Contractors.StatementOfWork.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class StatementOfWorkCycleChecks
{
    public static void StatementOfWorkCycleStartsInRequested()
    {
        var item = StatementOfWorkCycle.NewStatementOfWorkCycle("code-954", "label-200");
        Check.Equal(item.Status, StatementOfWorkCycleStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void StatementOfWorkCycleMovesToGranted()
    {
        var item = StatementOfWorkCycle.NewStatementOfWorkCycle("code-954", "label-200");
        try
        {
            item.MoveTo(StatementOfWorkCycleStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, StatementOfWorkCycleStatus.Granted, "status");
    }

    public static void StatementOfWorkCycleRejectsMovesFromExpired()
    {
        var item = StatementOfWorkCycle.NewStatementOfWorkCycle("code-954", "label-200");
        item.Status = StatementOfWorkCycleStatus.Expired;
        Check.Fails(() => item.MoveTo(StatementOfWorkCycleStatus.Requested), "move");
    }

    public static void RunAll()
    {
        StatementOfWorkCycleStartsInRequested();
        StatementOfWorkCycleMovesToGranted();
        StatementOfWorkCycleRejectsMovesFromExpired();
    }
}
