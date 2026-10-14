namespace Acme.Workforce.Talent.Tests.SkillProfile.Group;

using Acme.Workforce.Talent.SkillProfile.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class SkillProfileBudgetEngineChecks
{
    public static void SkillProfileBudgetEngineKeepsResultsWithinBounds()
    {
        var engine = SkillProfileBudgetEngine.DefaultSkillProfileBudgetEngine();
        var item = SkillProfileBudget.NewSkillProfileBudget("code-739", "title-144");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        SkillProfileBudgetEngineKeepsResultsWithinBounds();
    }
}
