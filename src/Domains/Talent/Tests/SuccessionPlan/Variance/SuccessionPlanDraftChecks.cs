namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Variance;

using Acme.Workforce.Talent.SuccessionPlan.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class SuccessionPlanDraftChecks
{
    public static void SuccessionPlanDraftStartsInCreated()
    {
        var item = SuccessionPlanDraft.NewSuccessionPlanDraft("code-326", "title-862");
        Check.Equal(item.Status, SuccessionPlanDraftStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SuccessionPlanDraftMovesToRetired()
    {
        var item = SuccessionPlanDraft.NewSuccessionPlanDraft("code-326", "title-862");
        try
        {
            item.MoveTo(SuccessionPlanDraftStatus.Retired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SuccessionPlanDraftStatus.Retired, "status");
    }

    public static void SuccessionPlanDraftRejectsMovesFromRetired()
    {
        var item = SuccessionPlanDraft.NewSuccessionPlanDraft("code-326", "title-862");
        item.Status = SuccessionPlanDraftStatus.Retired;
        Check.Fails(() => item.MoveTo(SuccessionPlanDraftStatus.Created), "move");
    }

    public static void RunAll()
    {
        SuccessionPlanDraftStartsInCreated();
        SuccessionPlanDraftMovesToRetired();
        SuccessionPlanDraftRejectsMovesFromRetired();
    }
}
