namespace Acme.Workforce.Orgdesign.Tests.Position.Entry;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Orgdesign.Position.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class PositionProfileEngineChecks
{
    public static void PositionProfileEngineKeepsResultsWithinBounds()
    {
        var engine = PositionProfileEngine.DefaultPositionProfileEngine(Flags.NewRegistry());
        var item = PositionProfile.NewPositionProfile("code-607", "name-436");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PositionProfileEngineKeepsResultsWithinBounds();
    }
}
