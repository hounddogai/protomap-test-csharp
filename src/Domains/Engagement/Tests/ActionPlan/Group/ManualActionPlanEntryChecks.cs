namespace Acme.Workforce.Engagement.Tests.ActionPlan.Group;

using Acme.Workforce.Engagement.ActionPlan.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class ManualActionPlanEntryChecks
{
    public static void ManualActionPlanEntryStartsInCreated()
    {
        var item = ManualActionPlanEntry.NewManualActionPlanEntry("code-354", "label-100");
        Check.Equal(item.Status, ManualActionPlanEntryStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ManualActionPlanEntryMovesToPublished()
    {
        var item = ManualActionPlanEntry.NewManualActionPlanEntry("code-354", "label-100");
        try
        {
            item.MoveTo(ManualActionPlanEntryStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ManualActionPlanEntryStatus.Published, "status");
    }

    public static void ManualActionPlanEntryRejectsMovesFromRetired()
    {
        var item = ManualActionPlanEntry.NewManualActionPlanEntry("code-354", "label-100");
        item.Status = ManualActionPlanEntryStatus.Retired;
        Check.Fails(() => item.MoveTo(ManualActionPlanEntryStatus.Created), "move");
    }

    public static void RunAll()
    {
        ManualActionPlanEntryStartsInCreated();
        ManualActionPlanEntryMovesToPublished();
        ManualActionPlanEntryRejectsMovesFromRetired();
    }
}
