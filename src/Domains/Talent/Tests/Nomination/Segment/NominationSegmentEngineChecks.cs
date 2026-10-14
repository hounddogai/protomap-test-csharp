namespace Acme.Workforce.Talent.Tests.Nomination.Segment;

using Acme.Workforce.Talent.Nomination.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class NominationSegmentEngineChecks
{
    public static void NominationSegmentEngineKeepsResultsWithinBounds()
    {
        var engine = NominationSegmentEngine.DefaultNominationSegmentEngine();
        var item = NominationSegment.NewNominationSegment("code-276", "title-757");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        NominationSegmentEngineKeepsResultsWithinBounds();
    }
}
