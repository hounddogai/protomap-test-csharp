namespace Acme.Workforce.Absence.Tests.ReturnPlan.Preference;

using Acme.Workforce.Absence.ReturnPlan.Preference;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class ReturnPlanProfileEngineChecks
{
    public static void ReturnPlanProfileEngineKeepsResultsWithinBounds()
    {
        var engine = ReturnPlanProfileEngine.DefaultReturnPlanProfileEngine(Flags.NewRegistry());
        var item = ReturnPlanProfile.NewReturnPlanProfile("code-863", "name-483");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ReturnPlanProfileEngineKeepsResultsWithinBounds();
    }
}
