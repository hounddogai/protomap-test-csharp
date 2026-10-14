namespace Acme.Workforce.Absence.Tests.Accrual.Schedule;

using Acme.Workforce.Absence.Accrual.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class AccrualTargetEngineChecks
{
    public static void AccrualTargetEngineKeepsResultsWithinBounds()
    {
        var engine = AccrualTargetEngine.DefaultAccrualTargetEngine();
        var item = AccrualTarget.NewAccrualTarget("code-866", "name-940");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AccrualTargetEngineKeepsResultsWithinBounds();
    }
}
