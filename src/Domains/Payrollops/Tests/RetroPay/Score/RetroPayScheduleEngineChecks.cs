namespace Acme.Workforce.Payrollops.Tests.RetroPay.Score;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Payrollops.RetroPay.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class RetroPayScheduleEngineChecks
{
    public static void RetroPayScheduleEngineKeepsResultsWithinBounds()
    {
        var engine = RetroPayScheduleEngine.DefaultRetroPayScheduleEngine(Flags.NewRegistry());
        var item = RetroPaySchedule.NewRetroPaySchedule("code-778", "label-940");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RetroPayScheduleEngineKeepsResultsWithinBounds();
    }
}
