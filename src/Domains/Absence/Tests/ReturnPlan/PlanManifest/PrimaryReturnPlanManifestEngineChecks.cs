namespace Acme.Workforce.Absence.Tests.ReturnPlan.PlanManifest;

using Acme.Workforce.Absence.ReturnPlan.PlanManifest;

/// <summary>Checks of plan manifest code, run by the project's runner.</summary>
public static class PrimaryReturnPlanManifestEngineChecks
{
    public static void PrimaryReturnPlanManifestEngineKeepsResultsWithinBounds()
    {
        var engine = PrimaryReturnPlanManifestEngine.DefaultPrimaryReturnPlanManifestEngine();
        var item = PrimaryReturnPlanManifest.NewPrimaryReturnPlanManifest("code-161", "title-482");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PrimaryReturnPlanManifestEngineKeepsResultsWithinBounds();
    }
}
