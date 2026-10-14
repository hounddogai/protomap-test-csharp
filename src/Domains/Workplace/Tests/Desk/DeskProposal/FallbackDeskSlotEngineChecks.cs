namespace Acme.Workforce.Workplace.Tests.Desk.DeskProposal;

using Acme.Workforce.Workplace.Desk.DeskProposal;

/// <summary>Checks of desk proposal code, run by the project's runner.</summary>
public static class FallbackDeskSlotEngineChecks
{
    public static void FallbackDeskSlotEngineKeepsResultsWithinBounds()
    {
        var engine = FallbackDeskSlotEngine.DefaultFallbackDeskSlotEngine();
        var item = FallbackDeskSlot.NewFallbackDeskSlot("code-171", "title-692");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        FallbackDeskSlotEngineKeepsResultsWithinBounds();
    }
}
