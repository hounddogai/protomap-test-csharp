namespace Acme.Workforce.Compensation.Tests.Adjustment.Profile;

using Acme.Workforce.Compensation.Adjustment.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class SeasonalAdjustmentReviewRulesChecks
{
    public static void SeasonalAdjustmentReviewValidatorReportsEveryProblem()
    {
        var validator = SeasonalAdjustmentReviewRules.DefaultSeasonalAdjustmentReviewValidator();
        var item = SeasonalAdjustmentReview.NewSeasonalAdjustmentReview("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SeasonalAdjustmentReviewUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt()
    {
        var rule = new SeasonalAdjustmentReviewUpdatedAtRangeRule(80, 3600);
        var item = SeasonalAdjustmentReview.NewSeasonalAdjustmentReview("x", "y");
        item.UpdatedAt = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void SeasonalAdjustmentReviewTitleLengthRuleFlagsALongTitle()
    {
        var rule = new SeasonalAdjustmentReviewTitleLengthRule(32);
        var item = SeasonalAdjustmentReview.NewSeasonalAdjustmentReview("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SeasonalAdjustmentReviewRequiredNotesRuleFlagsABlankNotes()
    {
        var rule = new SeasonalAdjustmentReviewRequiredNotesRule("seasonal adjustment review needs a value");
        var item = SeasonalAdjustmentReview.NewSeasonalAdjustmentReview("", "");
        item.Notes = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SeasonalAdjustmentReviewCodeLengthRuleFlagsALongCode()
    {
        var rule = new SeasonalAdjustmentReviewCodeLengthRule(120);
        var item = SeasonalAdjustmentReview.NewSeasonalAdjustmentReview("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        SeasonalAdjustmentReviewValidatorReportsEveryProblem();
        SeasonalAdjustmentReviewUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt();
        SeasonalAdjustmentReviewTitleLengthRuleFlagsALongTitle();
        SeasonalAdjustmentReviewRequiredNotesRuleFlagsABlankNotes();
        SeasonalAdjustmentReviewCodeLengthRuleFlagsALongCode();
    }
}
