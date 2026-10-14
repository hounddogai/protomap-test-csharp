namespace Acme.Workforce.Talent.Tests.Mobility.Allocation;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Talent.Mobility.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class RollingMobilityNoticeEngineChecks
{
    public static void RollingMobilityNoticeEngineKeepsResultsWithinBounds()
    {
        var engine = RollingMobilityNoticeEngine.DefaultRollingMobilityNoticeEngine(Flags.NewRegistry());
        var item = RollingMobilityNotice.NewRollingMobilityNotice("code-192", "name-894");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RollingMobilityNoticeEngineKeepsResultsWithinBounds();
    }
}
