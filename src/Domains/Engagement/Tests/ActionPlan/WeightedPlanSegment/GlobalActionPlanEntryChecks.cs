namespace Acme.Workforce.Engagement.Tests.ActionPlan.WeightedPlanSegment;

using Acme.Workforce.Engagement.ActionPlan.WeightedPlanSegment;

/// <summary>Checks of weighted plan segment code, run by the project's runner.</summary>
public static class GlobalActionPlanEntryChecks
{
    public static void GlobalActionPlanEntryStartsInDraft()
    {
        var item = GlobalActionPlanEntry.NewGlobalActionPlanEntry("code-111", "title-153");
        Check.Equal(item.Status, GlobalActionPlanEntryStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void GlobalActionPlanEntryMovesToActive()
    {
        var item = GlobalActionPlanEntry.NewGlobalActionPlanEntry("code-111", "title-153");
        try
        {
            item.MoveTo(GlobalActionPlanEntryStatus.Active);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, GlobalActionPlanEntryStatus.Active, "status");
    }

    public static void GlobalActionPlanEntryRejectsMovesFromClosed()
    {
        var item = GlobalActionPlanEntry.NewGlobalActionPlanEntry("code-111", "title-153");
        item.Status = GlobalActionPlanEntryStatus.Closed;
        Check.Fails(() => item.MoveTo(GlobalActionPlanEntryStatus.Draft), "move");
    }

    public static void RunAll()
    {
        GlobalActionPlanEntryStartsInDraft();
        GlobalActionPlanEntryMovesToActive();
        GlobalActionPlanEntryRejectsMovesFromClosed();
    }
}
