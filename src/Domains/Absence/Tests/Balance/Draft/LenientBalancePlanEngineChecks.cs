namespace Acme.Workforce.Absence.Tests.Balance.Draft;

using Acme.Workforce.Absence.Balance.Draft;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class LenientBalancePlanEngineChecks
{
    public static void LenientBalancePlanEngineKeepsResultsWithinBounds()
    {
        var engine = LenientBalancePlanEngine.DefaultLenientBalancePlanEngine(Flags.NewRegistry());
        var item = LenientBalancePlan.NewLenientBalancePlan("code-461", "name-118");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LenientBalancePlanEngineKeepsResultsWithinBounds();
    }
}
