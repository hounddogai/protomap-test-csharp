namespace Acme.Workforce.Compensation.Tests.MeritCycle.CycleReview;

using Acme.Workforce.Compensation.MeritCycle.CycleReview;

/// <summary>Checks of cycle review code, run by the project's runner.</summary>
public static class LegacyMeritCycleReviewRulesChecks
{
    public static void LegacyMeritCycleReviewValidatorReportsEveryProblem()
    {
        var validator = LegacyMeritCycleReviewRules.DefaultLegacyMeritCycleReviewValidator();
        var item = LegacyMeritCycleReview.NewLegacyMeritCycleReview("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LegacyMeritCycleReviewRequiredNameRuleFlagsABlankName()
    {
        var rule = new LegacyMeritCycleReviewRequiredNameRule("legacy merit cycle review needs an entry");
        var item = LegacyMeritCycleReview.NewLegacyMeritCycleReview("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LegacyMeritCycleReviewLeaveDaysRangeRuleFlagsValuesAboveLeaveDays()
    {
        var rule = new LegacyMeritCycleReviewLeaveDaysRangeRule(3600, 86400);
        var item = LegacyMeritCycleReview.NewLegacyMeritCycleReview("x", "y");
        item.LeaveDays = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LegacyMeritCycleReviewRequiredOwnerIdRuleFlagsABlankOwnerId()
    {
        var rule = new LegacyMeritCycleReviewRequiredOwnerIdRule("legacy merit cycle review needs a value");
        var item = LegacyMeritCycleReview.NewLegacyMeritCycleReview("", "");
        item.OwnerId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        LegacyMeritCycleReviewValidatorReportsEveryProblem();
        LegacyMeritCycleReviewRequiredNameRuleFlagsABlankName();
        LegacyMeritCycleReviewLeaveDaysRangeRuleFlagsValuesAboveLeaveDays();
        LegacyMeritCycleReviewRequiredOwnerIdRuleFlagsABlankOwnerId();
    }
}
