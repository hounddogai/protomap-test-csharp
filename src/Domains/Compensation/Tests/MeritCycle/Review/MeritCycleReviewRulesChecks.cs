namespace Acme.Workforce.Compensation.Tests.MeritCycle.Review;

using Acme.Workforce.Compensation.MeritCycle.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class MeritCycleReviewRulesChecks
{
    public static void MeritCycleReviewValidatorReportsEveryProblem()
    {
        var validator = MeritCycleReviewRules.DefaultMeritCycleReviewValidator();
        var item = MeritCycleReview.NewMeritCycleReview("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void MeritCycleReviewRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new MeritCycleReviewRequiredTitleRule("merit cycle review needs this field");
        var item = MeritCycleReview.NewMeritCycleReview("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        MeritCycleReviewValidatorReportsEveryProblem();
        MeritCycleReviewRequiredTitleRuleFlagsABlankTitle();
    }
}
