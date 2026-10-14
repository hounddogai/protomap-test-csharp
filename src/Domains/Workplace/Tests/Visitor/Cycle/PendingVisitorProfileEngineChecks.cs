namespace Acme.Workforce.Workplace.Tests.Visitor.Cycle;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Workplace.Visitor.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class PendingVisitorProfileEngineChecks
{
    public static void PendingVisitorProfileEngineKeepsResultsWithinBounds()
    {
        var engine = PendingVisitorProfileEngine.DefaultPendingVisitorProfileEngine(Flags.NewRegistry());
        var item = PendingVisitorProfile.NewPendingVisitorProfile("code-416", "title-123");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PendingVisitorProfileEngineKeepsResultsWithinBounds();
    }
}
