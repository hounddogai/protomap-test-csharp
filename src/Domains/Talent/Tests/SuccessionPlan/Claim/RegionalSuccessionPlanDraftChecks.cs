namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Claim;

using Acme.Workforce.Talent.SuccessionPlan.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class RegionalSuccessionPlanDraftChecks
{
    public static void RegionalSuccessionPlanDraftStartsInCreated()
    {
        var item = RegionalSuccessionPlanDraft.NewRegionalSuccessionPlanDraft("code-489", "title-653");
        Check.Equal(item.Status, RegionalSuccessionPlanDraftStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RegionalSuccessionPlanDraftMovesToValidated()
    {
        var item = RegionalSuccessionPlanDraft.NewRegionalSuccessionPlanDraft("code-489", "title-653");
        try
        {
            item.MoveTo(RegionalSuccessionPlanDraftStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RegionalSuccessionPlanDraftStatus.Validated, "status");
    }

    public static void RegionalSuccessionPlanDraftRejectsMovesFromRetired()
    {
        var item = RegionalSuccessionPlanDraft.NewRegionalSuccessionPlanDraft("code-489", "title-653");
        item.Status = RegionalSuccessionPlanDraftStatus.Retired;
        Check.Fails(() => item.MoveTo(RegionalSuccessionPlanDraftStatus.Created), "move");
    }

    public static void RunAll()
    {
        RegionalSuccessionPlanDraftStartsInCreated();
        RegionalSuccessionPlanDraftMovesToValidated();
        RegionalSuccessionPlanDraftRejectsMovesFromRetired();
    }
}
