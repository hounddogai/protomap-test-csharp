namespace Acme.Workforce.Payrollops.Tests.Garnishment.Target;

using Acme.Workforce.Payrollops.Garnishment.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class GarnishmentSegmentEngineChecks
{
    public static void GarnishmentSegmentEngineKeepsResultsWithinBounds()
    {
        var engine = GarnishmentSegmentEngine.DefaultGarnishmentSegmentEngine();
        var item = GarnishmentSegment.NewGarnishmentSegment("code-56", "label-94");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        GarnishmentSegmentEngineKeepsResultsWithinBounds();
    }
}
