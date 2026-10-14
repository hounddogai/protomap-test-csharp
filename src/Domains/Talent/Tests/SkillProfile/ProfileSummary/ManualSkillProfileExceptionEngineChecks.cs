namespace Acme.Workforce.Talent.Tests.SkillProfile.ProfileSummary;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Talent.SkillProfile.ProfileSummary;

/// <summary>Checks of profile summary code, run by the project's runner.</summary>
public static class ManualSkillProfileExceptionEngineChecks
{
    public static void ManualSkillProfileExceptionEngineKeepsResultsWithinBounds()
    {
        var engine = ManualSkillProfileExceptionEngine.DefaultManualSkillProfileExceptionEngine(Flags.NewRegistry());
        var item = ManualSkillProfileException.NewManualSkillProfileException("code-841", "name-705");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ManualSkillProfileExceptionEngineKeepsResultsWithinBounds();
    }
}
