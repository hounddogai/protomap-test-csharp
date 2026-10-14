namespace Acme.Workforce.Payrollops.Tests.Deduction.Exception;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Payrollops.Deduction.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class DeductionScheduleEngineChecks
{
    public static void DeductionScheduleEngineKeepsResultsWithinBounds()
    {
        var engine = DeductionScheduleEngine.DefaultDeductionScheduleEngine(Flags.NewRegistry());
        var item = DeductionSchedule.NewDeductionSchedule("code-584", "title-743");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        DeductionScheduleEngineKeepsResultsWithinBounds();
    }
}
