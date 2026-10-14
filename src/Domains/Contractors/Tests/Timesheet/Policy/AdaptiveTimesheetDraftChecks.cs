namespace Acme.Workforce.Contractors.Tests.Timesheet.Policy;

using Acme.Workforce.Contractors.Timesheet.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class AdaptiveTimesheetDraftChecks
{
    public static void AdaptiveTimesheetDraftStartsInRequested()
    {
        var item = AdaptiveTimesheetDraft.NewAdaptiveTimesheetDraft("code-242", "name-906");
        Check.Equal(item.Status, AdaptiveTimesheetDraftStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AdaptiveTimesheetDraftMovesToExpired()
    {
        var item = AdaptiveTimesheetDraft.NewAdaptiveTimesheetDraft("code-242", "name-906");
        try
        {
            item.MoveTo(AdaptiveTimesheetDraftStatus.Expired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AdaptiveTimesheetDraftStatus.Expired, "status");
    }

    public static void AdaptiveTimesheetDraftRejectsMovesFromExpired()
    {
        var item = AdaptiveTimesheetDraft.NewAdaptiveTimesheetDraft("code-242", "name-906");
        item.Status = AdaptiveTimesheetDraftStatus.Expired;
        Check.Fails(() => item.MoveTo(AdaptiveTimesheetDraftStatus.Requested), "move");
    }

    public static void RunAll()
    {
        AdaptiveTimesheetDraftStartsInRequested();
        AdaptiveTimesheetDraftMovesToExpired();
        AdaptiveTimesheetDraftRejectsMovesFromExpired();
    }
}
