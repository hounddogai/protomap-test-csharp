namespace Acme.Workforce.Compensation.Tests.Adjustment.Agreement;

using Acme.Workforce.Compensation.Adjustment.Agreement;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class LenientAdjustmentScheduleEngineChecks
{
    public static void LenientAdjustmentScheduleEngineKeepsResultsWithinBounds()
    {
        var engine = LenientAdjustmentScheduleEngine.DefaultLenientAdjustmentScheduleEngine(Flags.NewRegistry());
        var item = LenientAdjustmentSchedule.NewLenientAdjustmentSchedule("code-832", "name-223");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LenientAdjustmentScheduleEngineKeepsResultsWithinBounds();
    }
}
