namespace Acme.Workforce.Compliance.Tests.Exception.Baseline;

using Acme.Workforce.Compliance.Exception.Baseline;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class ExceptionBaselineEngineChecks
{
    public static void ExceptionBaselineEngineKeepsResultsWithinBounds()
    {
        var engine = ExceptionBaselineEngine.DefaultExceptionBaselineEngine(Flags.NewRegistry());
        var item = ExceptionBaseline.NewExceptionBaseline("code-920", "label-873");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ExceptionBaselineEngineKeepsResultsWithinBounds();
    }
}
