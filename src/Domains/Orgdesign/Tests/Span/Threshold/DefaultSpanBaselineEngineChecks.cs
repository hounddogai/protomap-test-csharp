namespace Acme.Workforce.Orgdesign.Tests.Span.Threshold;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Orgdesign.Span.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class DefaultSpanBaselineEngineChecks
{
    public static void DefaultSpanBaselineEngineKeepsResultsWithinBounds()
    {
        var engine = DefaultSpanBaselineEngine.DefaultDefaultSpanBaselineEngine(Flags.NewRegistry());
        var item = DefaultSpanBaseline.NewDefaultSpanBaseline("code-946", "name-22");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        DefaultSpanBaselineEngineKeepsResultsWithinBounds();
    }
}
