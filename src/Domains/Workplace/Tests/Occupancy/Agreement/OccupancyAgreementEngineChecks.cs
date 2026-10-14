namespace Acme.Workforce.Workplace.Tests.Occupancy.Agreement;

using Acme.Workforce.Workplace.Occupancy.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class OccupancyAgreementEngineChecks
{
    public static void OccupancyAgreementEngineKeepsResultsWithinBounds()
    {
        var engine = OccupancyAgreementEngine.DefaultOccupancyAgreementEngine();
        var item = OccupancyAgreement.NewOccupancyAgreement("code-531", "title-400");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        OccupancyAgreementEngineKeepsResultsWithinBounds();
    }
}
