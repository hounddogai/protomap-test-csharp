namespace Acme.Workforce.Compliance.Tests.Exception.ExceptionCheckpoint;

using Acme.Workforce.Compliance.Exception.ExceptionCheckpoint;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of exception checkpoint code, run by the project's runner.</summary>
public static class LegacyExceptionProfileEngineChecks
{
    public static void LegacyExceptionProfileEngineKeepsResultsWithinBounds()
    {
        var engine = LegacyExceptionProfileEngine.DefaultLegacyExceptionProfileEngine(Flags.NewRegistry());
        var item = LegacyExceptionProfile.NewLegacyExceptionProfile("code-294", "label-383");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LegacyExceptionProfileEngineKeepsResultsWithinBounds();
    }
}
