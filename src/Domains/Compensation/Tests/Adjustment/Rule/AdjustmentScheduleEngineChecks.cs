namespace Acme.Workforce.Compensation.Tests.Adjustment.Rule;

using Acme.Workforce.Compensation.Adjustment.Rule;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class AdjustmentScheduleEngineChecks
{
    public static void AdjustmentScheduleEngineKeepsResultsWithinBounds()
    {
        var engine = AdjustmentScheduleEngine.DefaultAdjustmentScheduleEngine(Flags.NewRegistry());
        var item = AdjustmentSchedule.NewAdjustmentSchedule("code-958", "label-869");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AdjustmentScheduleEngineKeepsResultsWithinBounds();
    }
}
