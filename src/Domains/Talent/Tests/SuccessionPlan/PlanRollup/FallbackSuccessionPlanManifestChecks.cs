namespace Acme.Workforce.Talent.Tests.SuccessionPlan.PlanRollup;

using Acme.Workforce.Talent.SuccessionPlan.PlanRollup;

/// <summary>Checks of plan rollup code, run by the project's runner.</summary>
public static class FallbackSuccessionPlanManifestChecks
{
    public static void FallbackSuccessionPlanManifestStartsInNew()
    {
        var item = FallbackSuccessionPlanManifest.NewFallbackSuccessionPlanManifest("code-353", "name-283");
        Check.Equal(item.Status, FallbackSuccessionPlanManifestStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FallbackSuccessionPlanManifestMovesToFailed()
    {
        var item = FallbackSuccessionPlanManifest.NewFallbackSuccessionPlanManifest("code-353", "name-283");
        try
        {
            item.MoveTo(FallbackSuccessionPlanManifestStatus.Failed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FallbackSuccessionPlanManifestStatus.Failed, "status");
    }

    public static void FallbackSuccessionPlanManifestRejectsMovesFromFailed()
    {
        var item = FallbackSuccessionPlanManifest.NewFallbackSuccessionPlanManifest("code-353", "name-283");
        item.Status = FallbackSuccessionPlanManifestStatus.Failed;
        Check.Fails(() => item.MoveTo(FallbackSuccessionPlanManifestStatus.New), "move");
    }

    public static void RunAll()
    {
        FallbackSuccessionPlanManifestStartsInNew();
        FallbackSuccessionPlanManifestMovesToFailed();
        FallbackSuccessionPlanManifestRejectsMovesFromFailed();
    }
}
