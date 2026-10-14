namespace Acme.Workforce.Talent.Tests.SkillProfile.Review;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Talent.SkillProfile.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class SkillProfileReviewEngineChecks
{
    public static void SkillProfileReviewEngineKeepsResultsWithinBounds()
    {
        var engine = SkillProfileReviewEngine.DefaultSkillProfileReviewEngine(Flags.NewRegistry());
        var item = SkillProfileReview.NewSkillProfileReview("code-232", "name-200");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        SkillProfileReviewEngineKeepsResultsWithinBounds();
    }
}
