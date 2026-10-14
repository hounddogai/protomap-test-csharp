namespace Acme.Workforce.Absence.Tests.ReturnPlan.Manifest;

using Acme.Workforce.Absence.ReturnPlan.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class ReturnPlanEntryChecks
{
    public static void ReturnPlanEntryStartsInProposed()
    {
        var item = ReturnPlanEntry.NewReturnPlanEntry("code-160", "title-60");
        Check.Equal(item.Status, ReturnPlanEntryStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ReturnPlanEntryMovesToRejected()
    {
        var item = ReturnPlanEntry.NewReturnPlanEntry("code-160", "title-60");
        try
        {
            item.MoveTo(ReturnPlanEntryStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ReturnPlanEntryStatus.Rejected, "status");
    }

    public static void ReturnPlanEntryRejectsMovesFromWithdrawn()
    {
        var item = ReturnPlanEntry.NewReturnPlanEntry("code-160", "title-60");
        item.Status = ReturnPlanEntryStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(ReturnPlanEntryStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        ReturnPlanEntryStartsInProposed();
        ReturnPlanEntryMovesToRejected();
        ReturnPlanEntryRejectsMovesFromWithdrawn();
    }
}
