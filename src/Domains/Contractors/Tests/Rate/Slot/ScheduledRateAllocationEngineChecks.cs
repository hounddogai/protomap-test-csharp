namespace Acme.Workforce.Contractors.Tests.Rate.Slot;

using Acme.Workforce.Contractors.Rate.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class ScheduledRateAllocationEngineChecks
{
    public static void ScheduledRateAllocationEngineKeepsResultsWithinBounds()
    {
        var engine = ScheduledRateAllocationEngine.DefaultScheduledRateAllocationEngine();
        var item = ScheduledRateAllocation.NewScheduledRateAllocation("code-638", "label-390");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ScheduledRateAllocationEngineKeepsResultsWithinBounds();
    }
}
