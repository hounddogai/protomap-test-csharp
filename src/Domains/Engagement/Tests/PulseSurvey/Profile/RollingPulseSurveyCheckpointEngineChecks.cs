namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Profile;

using Acme.Workforce.Engagement.PulseSurvey.Profile;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class RollingPulseSurveyCheckpointEngineChecks
{
    public static void RollingPulseSurveyCheckpointEngineKeepsResultsWithinBounds()
    {
        var engine = RollingPulseSurveyCheckpointEngine.DefaultRollingPulseSurveyCheckpointEngine(Flags.NewRegistry());
        var item = RollingPulseSurveyCheckpoint.NewRollingPulseSurveyCheckpoint("code-69", "title-188");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RollingPulseSurveyCheckpointEngineKeepsResultsWithinBounds();
    }
}
