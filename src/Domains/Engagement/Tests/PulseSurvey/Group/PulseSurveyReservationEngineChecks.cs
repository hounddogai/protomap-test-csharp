namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Group;

using Acme.Workforce.Engagement.PulseSurvey.Group;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class PulseSurveyReservationEngineChecks
{
    public static void PulseSurveyReservationEngineKeepsResultsWithinBounds()
    {
        var engine = PulseSurveyReservationEngine.DefaultPulseSurveyReservationEngine(Flags.NewRegistry());
        var item = PulseSurveyReservation.NewPulseSurveyReservation("code-524", "title-309");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PulseSurveyReservationEngineKeepsResultsWithinBounds();
    }
}
