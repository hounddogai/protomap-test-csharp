namespace Acme.Workforce.Orgdesign.Tests.Headcount.Slot;

using Acme.Workforce.Orgdesign.Headcount.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class HeadcountSlotEngineChecks
{
    public static void HeadcountSlotEngineKeepsResultsWithinBounds()
    {
        var engine = HeadcountSlotEngine.DefaultHeadcountSlotEngine();
        var item = HeadcountSlot.NewHeadcountSlot("code-7", "title-318");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        HeadcountSlotEngineKeepsResultsWithinBounds();
    }
}
