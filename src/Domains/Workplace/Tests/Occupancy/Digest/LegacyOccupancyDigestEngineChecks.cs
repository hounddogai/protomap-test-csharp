namespace Acme.Workforce.Workplace.Tests.Occupancy.Digest;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Workplace.Occupancy.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class LegacyOccupancyDigestEngineChecks
{
    public static void LegacyOccupancyDigestEngineKeepsResultsWithinBounds()
    {
        var engine = LegacyOccupancyDigestEngine.DefaultLegacyOccupancyDigestEngine(Flags.NewRegistry());
        var item = LegacyOccupancyDigest.NewLegacyOccupancyDigest("code-950", "name-756");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LegacyOccupancyDigestEngineKeepsResultsWithinBounds();
    }
}
