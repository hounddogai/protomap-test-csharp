namespace Acme.Workforce.Payrollops.Tests.Deduction.Segment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Payrollops.Deduction.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class ScheduledDeductionCycleEngineChecks
{
    public static void ScheduledDeductionCycleEngineKeepsResultsWithinBounds()
    {
        var engine = ScheduledDeductionCycleEngine.DefaultScheduledDeductionCycleEngine(Flags.NewRegistry());
        var item = ScheduledDeductionCycle.NewScheduledDeductionCycle("code-888", "title-178");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ScheduledDeductionCycleEngineKeepsResultsWithinBounds();
    }
}
