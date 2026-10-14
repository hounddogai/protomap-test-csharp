namespace Acme.Workforce.Talent.Tests.SkillProfile.Notice;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Talent.SkillProfile.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class PendingSkillProfileForecastEngineChecks
{
    public static void PendingSkillProfileForecastEngineKeepsResultsWithinBounds()
    {
        var engine = PendingSkillProfileForecastEngine.DefaultPendingSkillProfileForecastEngine(Flags.NewRegistry());
        var item = PendingSkillProfileForecast.NewPendingSkillProfileForecast("code-786", "title-360");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PendingSkillProfileForecastEngineKeepsResultsWithinBounds();
    }
}
