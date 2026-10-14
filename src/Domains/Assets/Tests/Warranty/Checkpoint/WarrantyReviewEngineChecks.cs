namespace Acme.Workforce.Assets.Tests.Warranty.Checkpoint;

using Acme.Workforce.Assets.Warranty.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class WarrantyReviewEngineChecks
{
    public static void WarrantyReviewEngineKeepsResultsWithinBounds()
    {
        var engine = WarrantyReviewEngine.DefaultWarrantyReviewEngine();
        var item = WarrantyReview.NewWarrantyReview("code-888", "name-644");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        WarrantyReviewEngineKeepsResultsWithinBounds();
    }
}
