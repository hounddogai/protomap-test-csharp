namespace Acme.Workforce.Assets.Tests.Assignment.Summary;

using Acme.Workforce.Assets.Assignment.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class RegionalAssignmentEntryChecks
{
    public static void RegionalAssignmentEntryStartsInOpen()
    {
        var item = RegionalAssignmentEntry.NewRegionalAssignmentEntry("code-91", "title-244");
        Check.Equal(item.Status, RegionalAssignmentEntryStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RegionalAssignmentEntryMovesToReopened()
    {
        var item = RegionalAssignmentEntry.NewRegionalAssignmentEntry("code-91", "title-244");
        try
        {
            item.MoveTo(RegionalAssignmentEntryStatus.Reopened);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RegionalAssignmentEntryStatus.Reopened, "status");
    }

    public static void RegionalAssignmentEntryRejectsMovesFromArchived()
    {
        var item = RegionalAssignmentEntry.NewRegionalAssignmentEntry("code-91", "title-244");
        item.Status = RegionalAssignmentEntryStatus.Archived;
        Check.Fails(() => item.MoveTo(RegionalAssignmentEntryStatus.Open), "move");
    }

    public static void RunAll()
    {
        RegionalAssignmentEntryStartsInOpen();
        RegionalAssignmentEntryMovesToReopened();
        RegionalAssignmentEntryRejectsMovesFromArchived();
    }
}
