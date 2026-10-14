namespace Acme.Workforce.Engagement.Tests.ActionPlan.Metric;

using Acme.Workforce.Engagement.ActionPlan.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class CappedActionPlanDraftChecks
{
    public static void CappedActionPlanDraftStartsInCreated()
    {
        var item = CappedActionPlanDraft.NewCappedActionPlanDraft("code-950", "name-918");
        Check.Equal(item.Status, CappedActionPlanDraftStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CappedActionPlanDraftMovesToValidated()
    {
        var item = CappedActionPlanDraft.NewCappedActionPlanDraft("code-950", "name-918");
        try
        {
            item.MoveTo(CappedActionPlanDraftStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CappedActionPlanDraftStatus.Validated, "status");
    }

    public static void CappedActionPlanDraftRejectsMovesFromRetired()
    {
        var item = CappedActionPlanDraft.NewCappedActionPlanDraft("code-950", "name-918");
        item.Status = CappedActionPlanDraftStatus.Retired;
        Check.Fails(() => item.MoveTo(CappedActionPlanDraftStatus.Created), "move");
    }

    public static void RunAll()
    {
        CappedActionPlanDraftStartsInCreated();
        CappedActionPlanDraftMovesToValidated();
        CappedActionPlanDraftRejectsMovesFromRetired();
    }
}
