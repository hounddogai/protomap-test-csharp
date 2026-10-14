namespace Acme.Workforce.Talent.Tests.SkillProfile.Target;

using Acme.Workforce.Talent.SkillProfile.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class LegacySkillProfileCheckpointEngineChecks
{
    public static void LegacySkillProfileCheckpointEngineKeepsResultsWithinBounds()
    {
        var engine = LegacySkillProfileCheckpointEngine.DefaultLegacySkillProfileCheckpointEngine();
        var item = LegacySkillProfileCheckpoint.NewLegacySkillProfileCheckpoint("code-696", "name-942");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LegacySkillProfileCheckpointEngineKeepsResultsWithinBounds();
    }
}
