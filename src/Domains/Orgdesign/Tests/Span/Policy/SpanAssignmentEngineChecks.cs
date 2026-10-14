namespace Acme.Workforce.Orgdesign.Tests.Span.Policy;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Orgdesign.Span.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class SpanAssignmentEngineChecks
{
    public static void SpanAssignmentEngineKeepsResultsWithinBounds()
    {
        var engine = SpanAssignmentEngine.DefaultSpanAssignmentEngine(Flags.NewRegistry());
        var item = SpanAssignment.NewSpanAssignment("code-320", "name-701");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        SpanAssignmentEngineKeepsResultsWithinBounds();
    }
}
