namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Group;

using Acme.Workforce.Engagement.PulseSurvey.Group;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class PulseSurveyGroupEngineChecks
{
    public static void PulseSurveyGroupEngineKeepsResultsWithinBounds()
    {
        var engine = PulseSurveyGroupEngine.DefaultPulseSurveyGroupEngine(Flags.NewRegistry());
        var item = PulseSurveyGroup.NewPulseSurveyGroup("code-13", "label-732");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PulseSurveyGroupEngineKeepsResultsWithinBounds();
    }
}
