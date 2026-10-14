namespace Acme.Workforce.Talent.Tests.CareerPath.Slot;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Talent.CareerPath.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class CareerPathSlotEngineChecks
{
    public static void CareerPathSlotEngineKeepsResultsWithinBounds()
    {
        var engine = CareerPathSlotEngine.DefaultCareerPathSlotEngine(Flags.NewRegistry());
        var item = CareerPathSlot.NewCareerPathSlot("code-328", "title-716");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        CareerPathSlotEngineKeepsResultsWithinBounds();
    }
}
