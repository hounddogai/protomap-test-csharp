namespace Acme.Workforce.Absence.Tests.ReturnPlan.Bundle;

using Acme.Workforce.Absence.ReturnPlan.Bundle;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class ReturnPlanCheckpointEngineChecks
{
    public static void ReturnPlanCheckpointEngineKeepsResultsWithinBounds()
    {
        var engine = ReturnPlanCheckpointEngine.DefaultReturnPlanCheckpointEngine(Flags.NewRegistry());
        var item = ReturnPlanCheckpoint.NewReturnPlanCheckpoint("code-55", "title-866");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ReturnPlanCheckpointEngineKeepsResultsWithinBounds();
    }
}
