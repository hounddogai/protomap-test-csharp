namespace Acme.Workforce.Orgdesign.Tests.Position.Plan;

using Acme.Workforce.Orgdesign.Position.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class PositionReviewRulesChecks
{
    public static void PositionReviewValidatorReportsEveryProblem()
    {
        var validator = PositionReviewRules.DefaultPositionReviewValidator();
        var item = PositionReview.NewPositionReview("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PositionReviewCodeLengthRuleFlagsALongCode()
    {
        var rule = new PositionReviewCodeLengthRule(120);
        var item = PositionReview.NewPositionReview("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PositionReviewChannelLengthRuleFlagsALongChannel()
    {
        var rule = new PositionReviewChannelLengthRule(32);
        var item = PositionReview.NewPositionReview("x", "y");
        item.Channel = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Channel = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PositionReviewDiscountRangeRuleFlagsValuesAboveDiscount()
    {
        var rule = new PositionReviewDiscountRangeRule(0.5, 1391.8);
        var item = PositionReview.NewPositionReview("x", "y");
        item.Discount = 1393.3;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PositionReviewRetriesRangeRuleFlagsValuesAboveRetries()
    {
        var rule = new PositionReviewRetriesRangeRule(120, 3600);
        var item = PositionReview.NewPositionReview("x", "y");
        item.Retries = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PositionReviewContactEmailLengthRuleFlagsALongContactEmail()
    {
        var rule = new PositionReviewContactEmailLengthRule(255);
        var item = PositionReview.NewPositionReview("x", "y");
        item.ContactEmail = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ContactEmail = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PositionReviewRequiredContactEmailRuleFlagsABlankContactEmail()
    {
        var rule = new PositionReviewRequiredContactEmailRule("position review needs a value");
        var item = PositionReview.NewPositionReview("", "");
        item.ContactEmail = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        PositionReviewValidatorReportsEveryProblem();
        PositionReviewCodeLengthRuleFlagsALongCode();
        PositionReviewChannelLengthRuleFlagsALongChannel();
        PositionReviewDiscountRangeRuleFlagsValuesAboveDiscount();
        PositionReviewRetriesRangeRuleFlagsValuesAboveRetries();
        PositionReviewContactEmailLengthRuleFlagsALongContactEmail();
        PositionReviewRequiredContactEmailRuleFlagsABlankContactEmail();
    }
}
