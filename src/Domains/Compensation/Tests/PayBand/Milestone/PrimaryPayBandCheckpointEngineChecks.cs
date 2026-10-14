namespace Acme.Workforce.Compensation.Tests.PayBand.Milestone;

using Acme.Workforce.Compensation.PayBand.Milestone;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class PrimaryPayBandCheckpointEngineChecks
{
    public static void PrimaryPayBandCheckpointEngineKeepsResultsWithinBounds()
    {
        var engine = PrimaryPayBandCheckpointEngine.DefaultPrimaryPayBandCheckpointEngine(Flags.NewRegistry());
        var item = PrimaryPayBandCheckpoint.NewPrimaryPayBandCheckpoint("code-226", "label-380");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PrimaryPayBandCheckpointEngineKeepsResultsWithinBounds();
    }
}
