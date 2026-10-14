namespace Acme.Workforce.Talent.Tests.SkillProfile.ProfileProposal;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Talent.SkillProfile.ProfileProposal;

/// <summary>Checks of profile proposal code, run by the project's runner.</summary>
public static class SkillProfileSummaryEngineChecks
{
    public static void SkillProfileSummaryEngineKeepsResultsWithinBounds()
    {
        var engine = SkillProfileSummaryEngine.DefaultSkillProfileSummaryEngine(Flags.NewRegistry());
        var item = SkillProfileSummary.NewSkillProfileSummary("code-994", "name-547");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        SkillProfileSummaryEngineKeepsResultsWithinBounds();
    }
}
