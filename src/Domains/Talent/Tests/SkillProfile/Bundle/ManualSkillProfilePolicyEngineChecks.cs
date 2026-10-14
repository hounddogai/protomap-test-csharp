namespace Acme.Workforce.Talent.Tests.SkillProfile.Bundle;

using Acme.Workforce.Talent.SkillProfile.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class ManualSkillProfilePolicyEngineChecks
{
    public static void ManualSkillProfilePolicyEngineKeepsResultsWithinBounds()
    {
        var engine = ManualSkillProfilePolicyEngine.DefaultManualSkillProfilePolicyEngine();
        var item = ManualSkillProfilePolicy.NewManualSkillProfilePolicy("code-891", "title-203");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ManualSkillProfilePolicyEngineKeepsResultsWithinBounds();
    }
}
