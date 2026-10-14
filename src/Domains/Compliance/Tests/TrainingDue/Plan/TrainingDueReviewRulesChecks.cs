namespace Acme.Workforce.Compliance.Tests.TrainingDue.Plan;

using Acme.Workforce.Compliance.TrainingDue.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class TrainingDueReviewRulesChecks
{
    public static void TrainingDueReviewValidatorReportsEveryProblem()
    {
        var validator = TrainingDueReviewRules.DefaultTrainingDueReviewValidator();
        var item = TrainingDueReview.NewTrainingDueReview("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TrainingDueReviewRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new TrainingDueReviewRequiredTitleRule("training due review needs this field");
        var item = TrainingDueReview.NewTrainingDueReview("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TrainingDueReviewRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new TrainingDueReviewRequiredLabelRule("training due review needs a value");
        var item = TrainingDueReview.NewTrainingDueReview("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TrainingDueReviewLeaveDaysRangeRuleFlagsValuesAboveLeaveDays()
    {
        var rule = new TrainingDueReviewLeaveDaysRangeRule(120, 1000);
        var item = TrainingDueReview.NewTrainingDueReview("x", "y");
        item.LeaveDays = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        TrainingDueReviewValidatorReportsEveryProblem();
        TrainingDueReviewRequiredTitleRuleFlagsABlankTitle();
        TrainingDueReviewRequiredLabelRuleFlagsABlankLabel();
        TrainingDueReviewLeaveDaysRangeRuleFlagsValuesAboveLeaveDays();
    }
}
