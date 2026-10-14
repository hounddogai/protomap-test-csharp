namespace Acme.Workforce.Assets.Tests.Laptop.Estimate;

using Acme.Workforce.Assets.Laptop.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class BatchedLaptopRequestEngineChecks
{
    public static void BatchedLaptopRequestEngineKeepsResultsWithinBounds()
    {
        var engine = BatchedLaptopRequestEngine.DefaultBatchedLaptopRequestEngine();
        var item = BatchedLaptopRequest.NewBatchedLaptopRequest("code-336", "label-114");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        BatchedLaptopRequestEngineKeepsResultsWithinBounds();
    }
}
