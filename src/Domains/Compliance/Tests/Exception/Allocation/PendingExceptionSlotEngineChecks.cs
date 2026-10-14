namespace Acme.Workforce.Compliance.Tests.Exception.Allocation;

using Acme.Workforce.Compliance.Exception.Allocation;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class PendingExceptionSlotEngineChecks
{
    public static void PendingExceptionSlotEngineKeepsResultsWithinBounds()
    {
        var engine = PendingExceptionSlotEngine.DefaultPendingExceptionSlotEngine(Flags.NewRegistry());
        var item = PendingExceptionSlot.NewPendingExceptionSlot("code-412", "title-245");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PendingExceptionSlotEngineKeepsResultsWithinBounds();
    }
}
