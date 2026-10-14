namespace Acme.Workforce.Absence.Tests.ReturnPlan.Agreement;

using Acme.Workforce.Absence.ReturnPlan.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class WeightedReturnPlanManifestChecks
{
    public static void WeightedReturnPlanManifestStartsInCreated()
    {
        var item = WeightedReturnPlanManifest.NewWeightedReturnPlanManifest("code-991", "name-862");
        Check.Equal(item.Status, WeightedReturnPlanManifestStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void WeightedReturnPlanManifestMovesToPublished()
    {
        var item = WeightedReturnPlanManifest.NewWeightedReturnPlanManifest("code-991", "name-862");
        try
        {
            item.MoveTo(WeightedReturnPlanManifestStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, WeightedReturnPlanManifestStatus.Published, "status");
    }

    public static void WeightedReturnPlanManifestRejectsMovesFromRetired()
    {
        var item = WeightedReturnPlanManifest.NewWeightedReturnPlanManifest("code-991", "name-862");
        item.Status = WeightedReturnPlanManifestStatus.Retired;
        Check.Fails(() => item.MoveTo(WeightedReturnPlanManifestStatus.Created), "move");
    }

    public static void RunAll()
    {
        WeightedReturnPlanManifestStartsInCreated();
        WeightedReturnPlanManifestMovesToPublished();
        WeightedReturnPlanManifestRejectsMovesFromRetired();
    }
}
