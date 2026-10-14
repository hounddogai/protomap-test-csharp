namespace Acme.Workforce.Talent.Tests.SkillProfile.Notice;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Talent.SkillProfile.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class SkillProfileNoticeEngineChecks
{
    public static void SkillProfileNoticeEngineKeepsResultsWithinBounds()
    {
        var engine = SkillProfileNoticeEngine.DefaultSkillProfileNoticeEngine(Flags.NewRegistry());
        var item = SkillProfileNotice.NewSkillProfileNotice("code-446", "title-602");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        SkillProfileNoticeEngineKeepsResultsWithinBounds();
    }
}
