namespace Acme.Workforce.Talent.Tests.SkillProfile.Target;

using Acme.Workforce.Talent.SkillProfile.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class SkillProfilePlanEngineChecks
{
    public static void SkillProfilePlanEngineKeepsResultsWithinBounds()
    {
        var engine = SkillProfilePlanEngine.DefaultSkillProfilePlanEngine();
        var item = SkillProfilePlan.NewSkillProfilePlan("code-760", "label-52");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        SkillProfilePlanEngineKeepsResultsWithinBounds();
    }
}
