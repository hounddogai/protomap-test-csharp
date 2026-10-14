namespace Acme.Workforce.Workplace.Tests.Desk.Variance;

using Acme.Workforce.Workplace.Desk.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class DeskEstimateEngineChecks
{
    public static void DeskEstimateEngineKeepsResultsWithinBounds()
    {
        var engine = DeskEstimateEngine.DefaultDeskEstimateEngine();
        var item = DeskEstimate.NewDeskEstimate("code-389", "title-898");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        DeskEstimateEngineKeepsResultsWithinBounds();
    }
}
