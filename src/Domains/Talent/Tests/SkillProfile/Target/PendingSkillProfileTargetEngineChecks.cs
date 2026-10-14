namespace Acme.Workforce.Talent.Tests.SkillProfile.Target;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Talent.SkillProfile.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class PendingSkillProfileTargetEngineChecks
{
    public static void PendingSkillProfileTargetEngineKeepsResultsWithinBounds()
    {
        var engine = PendingSkillProfileTargetEngine.DefaultPendingSkillProfileTargetEngine(Flags.NewRegistry());
        var item = PendingSkillProfileTarget.NewPendingSkillProfileTarget("code-46", "name-390");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PendingSkillProfileTargetEngineKeepsResultsWithinBounds();
    }
}
