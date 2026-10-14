namespace Acme.Workforce.Absence.Tests.ReturnPlan.PlanManifest;

using Acme.Workforce.Absence.ReturnPlan.PlanManifest;

/// <summary>Checks of plan manifest code, run by the project's runner.</summary>
public static class PrimaryReturnPlanManifestChecks
{
    public static void PrimaryReturnPlanManifestStartsInOpen()
    {
        var item = PrimaryReturnPlanManifest.NewPrimaryReturnPlanManifest("code-161", "title-482");
        Check.Equal(item.Status, PrimaryReturnPlanManifestStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PrimaryReturnPlanManifestMovesToResolved()
    {
        var item = PrimaryReturnPlanManifest.NewPrimaryReturnPlanManifest("code-161", "title-482");
        try
        {
            item.MoveTo(PrimaryReturnPlanManifestStatus.Resolved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PrimaryReturnPlanManifestStatus.Resolved, "status");
    }

    public static void PrimaryReturnPlanManifestRejectsMovesFromArchived()
    {
        var item = PrimaryReturnPlanManifest.NewPrimaryReturnPlanManifest("code-161", "title-482");
        item.Status = PrimaryReturnPlanManifestStatus.Archived;
        Check.Fails(() => item.MoveTo(PrimaryReturnPlanManifestStatus.Open), "move");
    }

    public static void RunAll()
    {
        PrimaryReturnPlanManifestStartsInOpen();
        PrimaryReturnPlanManifestMovesToResolved();
        PrimaryReturnPlanManifestRejectsMovesFromArchived();
    }
}
