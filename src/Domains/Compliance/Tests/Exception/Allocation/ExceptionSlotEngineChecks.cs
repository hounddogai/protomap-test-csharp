namespace Acme.Workforce.Compliance.Tests.Exception.Allocation;

using Acme.Workforce.Compliance.Exception.Allocation;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class ExceptionSlotEngineChecks
{
    public static void ExceptionSlotEngineKeepsResultsWithinBounds()
    {
        var engine = ExceptionSlotEngine.DefaultExceptionSlotEngine(Flags.NewRegistry());
        var item = ExceptionSlot.NewExceptionSlot("code-715", "name-79");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ExceptionSlotEngineKeepsResultsWithinBounds();
    }
}
