namespace Acme.Workforce.Talent.Tests.SkillProfile.Entry;

using Acme.Workforce.Talent.SkillProfile.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class SkillProfileEntryEngineChecks
{
    public static void SkillProfileEntryEngineKeepsResultsWithinBounds()
    {
        var engine = SkillProfileEntryEngine.DefaultSkillProfileEntryEngine();
        var item = SkillProfileEntry.NewSkillProfileEntry("code-929", "label-613");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        SkillProfileEntryEngineKeepsResultsWithinBounds();
    }
}
