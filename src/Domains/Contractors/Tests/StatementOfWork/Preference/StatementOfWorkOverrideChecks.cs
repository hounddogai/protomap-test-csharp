namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Preference;

using Acme.Workforce.Contractors.StatementOfWork.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class StatementOfWorkOverrideChecks
{
    public static void StatementOfWorkOverrideStartsInCreated()
    {
        var item = StatementOfWorkOverride.NewStatementOfWorkOverride("code-254", "label-302");
        Check.Equal(item.Status, StatementOfWorkOverrideStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void StatementOfWorkOverrideMovesToPublished()
    {
        var item = StatementOfWorkOverride.NewStatementOfWorkOverride("code-254", "label-302");
        try
        {
            item.MoveTo(StatementOfWorkOverrideStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, StatementOfWorkOverrideStatus.Published, "status");
    }

    public static void StatementOfWorkOverrideRejectsMovesFromRetired()
    {
        var item = StatementOfWorkOverride.NewStatementOfWorkOverride("code-254", "label-302");
        item.Status = StatementOfWorkOverrideStatus.Retired;
        Check.Fails(() => item.MoveTo(StatementOfWorkOverrideStatus.Created), "move");
    }

    public static void RunAll()
    {
        StatementOfWorkOverrideStartsInCreated();
        StatementOfWorkOverrideMovesToPublished();
        StatementOfWorkOverrideRejectsMovesFromRetired();
    }
}
