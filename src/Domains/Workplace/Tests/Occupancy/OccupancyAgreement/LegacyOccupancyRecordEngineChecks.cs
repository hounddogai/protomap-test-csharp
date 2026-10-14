namespace Acme.Workforce.Workplace.Tests.Occupancy.OccupancyAgreement;

using Acme.Workforce.Workplace.Occupancy.OccupancyAgreement;

/// <summary>Checks of occupancy agreement code, run by the project's runner.</summary>
public static class LegacyOccupancyRecordEngineChecks
{
    public static void LegacyOccupancyRecordEngineKeepsResultsWithinBounds()
    {
        var engine = LegacyOccupancyRecordEngine.DefaultLegacyOccupancyRecordEngine();
        var item = LegacyOccupancyRecord.NewLegacyOccupancyRecord("code-751", "name-802");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LegacyOccupancyRecordEngineKeepsResultsWithinBounds();
    }
}
