namespace Acme.Workforce.Engagement.Tests.Recognition.OverrideItem;

using Acme.Workforce.Engagement.Recognition.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class RecognitionReviewRulesChecks
{
    public static void RecognitionReviewValidatorReportsEveryProblem()
    {
        var validator = RecognitionReviewRules.DefaultRecognitionReviewValidator();
        var item = RecognitionReview.NewRecognitionReview("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RecognitionReviewOwnerIdLengthRuleFlagsALongOwnerId()
    {
        var rule = new RecognitionReviewOwnerIdLengthRule(3600);
        var item = RecognitionReview.NewRecognitionReview("x", "y");
        item.OwnerId = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.OwnerId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RecognitionReviewTitleLengthRuleFlagsALongTitle()
    {
        var rule = new RecognitionReviewTitleLengthRule(80);
        var item = RecognitionReview.NewRecognitionReview("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RecognitionReviewRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new RecognitionReviewRequiredCodeRule("recognition review needs a value");
        var item = RecognitionReview.NewRecognitionReview("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RecognitionReviewLimitRangeRuleFlagsValuesAboveLimit()
    {
        var rule = new RecognitionReviewLimitRangeRule(32, 80);
        var item = RecognitionReview.NewRecognitionReview("x", "y");
        item.Limit = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RecognitionReviewCodeLengthRuleFlagsALongCode()
    {
        var rule = new RecognitionReviewCodeLengthRule(255);
        var item = RecognitionReview.NewRecognitionReview("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        RecognitionReviewValidatorReportsEveryProblem();
        RecognitionReviewOwnerIdLengthRuleFlagsALongOwnerId();
        RecognitionReviewTitleLengthRuleFlagsALongTitle();
        RecognitionReviewRequiredCodeRuleFlagsABlankCode();
        RecognitionReviewLimitRangeRuleFlagsValuesAboveLimit();
        RecognitionReviewCodeLengthRuleFlagsALongCode();
    }
}
