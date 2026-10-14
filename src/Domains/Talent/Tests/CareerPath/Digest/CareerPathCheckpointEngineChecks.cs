namespace Acme.Workforce.Talent.Tests.CareerPath.Digest;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Talent.CareerPath.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class CareerPathCheckpointEngineChecks
{
    public static void CareerPathCheckpointEngineKeepsResultsWithinBounds()
    {
        var engine = CareerPathCheckpointEngine.DefaultCareerPathCheckpointEngine(Flags.NewRegistry());
        var item = CareerPathCheckpoint.NewCareerPathCheckpoint("code-395", "label-730");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        CareerPathCheckpointEngineKeepsResultsWithinBounds();
    }
}
