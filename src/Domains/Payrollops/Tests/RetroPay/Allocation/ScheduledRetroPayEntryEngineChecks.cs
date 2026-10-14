namespace Acme.Workforce.Payrollops.Tests.RetroPay.Allocation;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Payrollops.RetroPay.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class ScheduledRetroPayEntryEngineChecks
{
    public static void ScheduledRetroPayEntryEngineKeepsResultsWithinBounds()
    {
        var engine = ScheduledRetroPayEntryEngine.DefaultScheduledRetroPayEntryEngine(Flags.NewRegistry());
        var item = ScheduledRetroPayEntry.NewScheduledRetroPayEntry("code-134", "title-273");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ScheduledRetroPayEntryEngineKeepsResultsWithinBounds();
    }
}
