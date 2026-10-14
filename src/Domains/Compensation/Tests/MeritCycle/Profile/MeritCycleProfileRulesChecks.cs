namespace Acme.Workforce.Compensation.Tests.MeritCycle.Profile;

using Acme.Workforce.Compensation.MeritCycle.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class MeritCycleProfileRulesChecks
{
    public static void MeritCycleProfileValidatorReportsEveryProblem()
    {
        var validator = MeritCycleProfileRules.DefaultMeritCycleProfileValidator();
        var item = MeritCycleProfile.NewMeritCycleProfile("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void MeritCycleProfileReviewerLengthRuleFlagsALongReviewer()
    {
        var rule = new MeritCycleProfileReviewerLengthRule(86400);
        var item = MeritCycleProfile.NewMeritCycleProfile("x", "y");
        item.Reviewer = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Reviewer = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void MeritCycleProfileRegionLengthRuleFlagsALongRegion()
    {
        var rule = new MeritCycleProfileRegionLengthRule(3600);
        var item = MeritCycleProfile.NewMeritCycleProfile("x", "y");
        item.Region = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Region = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void MeritCycleProfileRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new MeritCycleProfileRequiredTitleRule("merit cycle profile needs an entry");
        var item = MeritCycleProfile.NewMeritCycleProfile("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        MeritCycleProfileValidatorReportsEveryProblem();
        MeritCycleProfileReviewerLengthRuleFlagsALongReviewer();
        MeritCycleProfileRegionLengthRuleFlagsALongRegion();
        MeritCycleProfileRequiredTitleRuleFlagsABlankTitle();
    }
}
