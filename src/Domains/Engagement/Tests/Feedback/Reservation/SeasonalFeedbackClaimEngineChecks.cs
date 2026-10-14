namespace Acme.Workforce.Engagement.Tests.Feedback.Reservation;

using Acme.Workforce.Engagement.Feedback.Reservation;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class SeasonalFeedbackClaimEngineChecks
{
    public static void SeasonalFeedbackClaimEngineKeepsResultsWithinBounds()
    {
        var engine = SeasonalFeedbackClaimEngine.DefaultSeasonalFeedbackClaimEngine(Flags.NewRegistry());
        var item = SeasonalFeedbackClaim.NewSeasonalFeedbackClaim("code-619", "title-251");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        SeasonalFeedbackClaimEngineKeepsResultsWithinBounds();
    }
}
