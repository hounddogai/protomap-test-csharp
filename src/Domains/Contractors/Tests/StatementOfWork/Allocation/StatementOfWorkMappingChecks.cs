namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Allocation;

using Acme.Workforce.Contractors.StatementOfWork.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class StatementOfWorkMappingChecks
{
    public static void StatementOfWorkMappingStartsInRequested()
    {
        var item = StatementOfWorkMapping.NewStatementOfWorkMapping("code-45", "name-90");
        Check.Equal(item.Status, StatementOfWorkMappingStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void StatementOfWorkMappingMovesToGranted()
    {
        var item = StatementOfWorkMapping.NewStatementOfWorkMapping("code-45", "name-90");
        try
        {
            item.MoveTo(StatementOfWorkMappingStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, StatementOfWorkMappingStatus.Granted, "status");
    }

    public static void StatementOfWorkMappingRejectsMovesFromExpired()
    {
        var item = StatementOfWorkMapping.NewStatementOfWorkMapping("code-45", "name-90");
        item.Status = StatementOfWorkMappingStatus.Expired;
        Check.Fails(() => item.MoveTo(StatementOfWorkMappingStatus.Requested), "move");
    }

    public static void RunAll()
    {
        StatementOfWorkMappingStartsInRequested();
        StatementOfWorkMappingMovesToGranted();
        StatementOfWorkMappingRejectsMovesFromExpired();
    }
}
