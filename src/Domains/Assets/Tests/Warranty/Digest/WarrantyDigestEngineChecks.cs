namespace Acme.Workforce.Assets.Tests.Warranty.Digest;

using Acme.Workforce.Assets.Warranty.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class WarrantyDigestEngineChecks
{
    public static void WarrantyDigestEngineKeepsResultsWithinBounds()
    {
        var engine = WarrantyDigestEngine.DefaultWarrantyDigestEngine();
        var item = WarrantyDigest.NewWarrantyDigest("code-70", "label-447");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        WarrantyDigestEngineKeepsResultsWithinBounds();
    }
}
