namespace Acme.Workforce.Talent.Tests.SkillProfile.Preference;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Talent.SkillProfile.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class RegionalSkillProfileRecordEngineChecks
{
    public static void RegionalSkillProfileRecordEngineKeepsResultsWithinBounds()
    {
        var engine = RegionalSkillProfileRecordEngine.DefaultRegionalSkillProfileRecordEngine(Flags.NewRegistry());
        var item = RegionalSkillProfileRecord.NewRegionalSkillProfileRecord("code-250", "label-318");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RegionalSkillProfileRecordEngineKeepsResultsWithinBounds();
    }
}
