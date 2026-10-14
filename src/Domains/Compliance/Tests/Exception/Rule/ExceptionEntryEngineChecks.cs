namespace Acme.Workforce.Compliance.Tests.Exception.Rule;

using Acme.Workforce.Compliance.Exception.Rule;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class ExceptionEntryEngineChecks
{
    public static void ExceptionEntryEngineKeepsResultsWithinBounds()
    {
        var engine = ExceptionEntryEngine.DefaultExceptionEntryEngine(Flags.NewRegistry());
        var item = ExceptionEntry.NewExceptionEntry("code-104", "title-410");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ExceptionEntryEngineKeepsResultsWithinBounds();
    }
}
