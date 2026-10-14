namespace Acme.Workforce.Talent.Tests.Mobility.Forecast;

using Acme.Workforce.Talent.Mobility.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class MobilityReviewEngineChecks
{
    public static void MobilityReviewEngineKeepsResultsWithinBounds()
    {
        var engine = MobilityReviewEngine.DefaultMobilityReviewEngine();
        var item = MobilityReview.NewMobilityReview("code-547", "label-723");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        MobilityReviewEngineKeepsResultsWithinBounds();
    }
}
