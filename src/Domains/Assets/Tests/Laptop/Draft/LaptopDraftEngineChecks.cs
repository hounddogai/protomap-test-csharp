namespace Acme.Workforce.Assets.Tests.Laptop.Draft;

using Acme.Workforce.Assets.Laptop.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class LaptopDraftEngineChecks
{
    public static void LaptopDraftEngineKeepsResultsWithinBounds()
    {
        var engine = LaptopDraftEngine.DefaultLaptopDraftEngine();
        var item = LaptopDraft.NewLaptopDraft("code-462", "label-802");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LaptopDraftEngineKeepsResultsWithinBounds();
    }
}
