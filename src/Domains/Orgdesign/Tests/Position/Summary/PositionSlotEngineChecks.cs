namespace Acme.Workforce.Orgdesign.Tests.Position.Summary;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Orgdesign.Position.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class PositionSlotEngineChecks
{
    public static void PositionSlotEngineKeepsResultsWithinBounds()
    {
        var engine = PositionSlotEngine.DefaultPositionSlotEngine(Flags.NewRegistry());
        var item = PositionSlot.NewPositionSlot("code-319", "name-598");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PositionSlotEngineKeepsResultsWithinBounds();
    }
}
