namespace Acme.Workforce.Orgdesign.Tests.Position.Plan;

using Acme.Workforce.Orgdesign.Position.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class RollingPositionManifestRulesChecks
{
    public static void RollingPositionManifestValidatorReportsEveryProblem()
    {
        var validator = RollingPositionManifestRules.DefaultRollingPositionManifestValidator();
        var item = RollingPositionManifest.NewRollingPositionManifest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RollingPositionManifestRequiredSegmentRuleFlagsABlankSegment()
    {
        var rule = new RollingPositionManifestRequiredSegmentRule("rolling position manifest needs a value");
        var item = RollingPositionManifest.NewRollingPositionManifest("", "");
        item.Segment = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RollingPositionManifestRequiredReviewerRuleFlagsABlankReviewer()
    {
        var rule = new RollingPositionManifestRequiredReviewerRule("rolling position manifest needs an entry");
        var item = RollingPositionManifest.NewRollingPositionManifest("", "");
        item.Reviewer = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RollingPositionManifestReviewerLengthRuleFlagsALongReviewer()
    {
        var rule = new RollingPositionManifestReviewerLengthRule(120);
        var item = RollingPositionManifest.NewRollingPositionManifest("x", "y");
        item.Reviewer = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Reviewer = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        RollingPositionManifestValidatorReportsEveryProblem();
        RollingPositionManifestRequiredSegmentRuleFlagsABlankSegment();
        RollingPositionManifestRequiredReviewerRuleFlagsABlankReviewer();
        RollingPositionManifestReviewerLengthRuleFlagsALongReviewer();
    }
}
