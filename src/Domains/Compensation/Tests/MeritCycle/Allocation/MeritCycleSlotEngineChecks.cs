namespace Acme.Workforce.Compensation.Tests.MeritCycle.Allocation;

using Acme.Workforce.Compensation.MeritCycle.Allocation;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class MeritCycleSlotEngineChecks
{
    public static void MeritCycleSlotEngineKeepsResultsWithinBounds()
    {
        var engine = MeritCycleSlotEngine.DefaultMeritCycleSlotEngine(Flags.NewRegistry());
        var item = MeritCycleSlot.NewMeritCycleSlot("code-162", "title-761");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        MeritCycleSlotEngineKeepsResultsWithinBounds();
    }
}
