namespace Acme.Workforce.Assets.Tests.Warranty.Forecast;

using Acme.Workforce.Assets.Warranty.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class WarrantyRequestEngineChecks
{
    public static void WarrantyRequestEngineKeepsResultsWithinBounds()
    {
        var engine = WarrantyRequestEngine.DefaultWarrantyRequestEngine();
        var item = WarrantyRequest.NewWarrantyRequest("code-939", "title-663");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        WarrantyRequestEngineKeepsResultsWithinBounds();
    }
}
