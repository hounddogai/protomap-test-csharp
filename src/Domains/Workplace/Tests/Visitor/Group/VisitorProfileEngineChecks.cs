namespace Acme.Workforce.Workplace.Tests.Visitor.Group;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Workplace.Visitor.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class VisitorProfileEngineChecks
{
    public static void VisitorProfileEngineKeepsResultsWithinBounds()
    {
        var engine = VisitorProfileEngine.DefaultVisitorProfileEngine(Flags.NewRegistry());
        var item = VisitorProfile.NewVisitorProfile("code-942", "label-876");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        VisitorProfileEngineKeepsResultsWithinBounds();
    }
}
