namespace Acme.Workforce.Assets.Tests.Warranty.Digest;

using Acme.Workforce.Assets.Warranty.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class StrictWarrantyDraftEngineChecks
{
    public static void StrictWarrantyDraftEngineKeepsResultsWithinBounds()
    {
        var engine = StrictWarrantyDraftEngine.DefaultStrictWarrantyDraftEngine();
        var item = StrictWarrantyDraft.NewStrictWarrantyDraft("code-374", "name-540");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        StrictWarrantyDraftEngineKeepsResultsWithinBounds();
    }
}
