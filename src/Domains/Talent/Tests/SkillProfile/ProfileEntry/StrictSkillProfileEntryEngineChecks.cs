namespace Acme.Workforce.Talent.Tests.SkillProfile.ProfileEntry;

using Acme.Workforce.Talent.SkillProfile.ProfileEntry;

/// <summary>Checks of profile entry code, run by the project's runner.</summary>
public static class StrictSkillProfileEntryEngineChecks
{
    public static void StrictSkillProfileEntryEngineKeepsResultsWithinBounds()
    {
        var engine = StrictSkillProfileEntryEngine.DefaultStrictSkillProfileEntryEngine();
        var item = StrictSkillProfileEntry.NewStrictSkillProfileEntry("code-382", "title-180");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        StrictSkillProfileEntryEngineKeepsResultsWithinBounds();
    }
}
