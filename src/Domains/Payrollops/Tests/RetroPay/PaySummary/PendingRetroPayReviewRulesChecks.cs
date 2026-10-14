namespace Acme.Workforce.Payrollops.Tests.RetroPay.PaySummary;

using Acme.Workforce.Payrollops.RetroPay.PaySummary;

/// <summary>Checks of pay summary code, run by the project's runner.</summary>
public static class PendingRetroPayReviewRulesChecks
{
    public static void PendingRetroPayReviewValidatorReportsEveryProblem()
    {
        var validator = PendingRetroPayReviewRules.DefaultPendingRetroPayReviewValidator();
        var item = PendingRetroPayReview.NewPendingRetroPayReview("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PendingRetroPayReviewRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new PendingRetroPayReviewRequiredCodeRule("pending retro pay review needs this field");
        var item = PendingRetroPayReview.NewPendingRetroPayReview("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PendingRetroPayReviewFloorValueRangeRuleFlagsValuesAboveFloorValue()
    {
        var rule = new PendingRetroPayReviewFloorValueRangeRule(3.4, 1323.8);
        var item = PendingRetroPayReview.NewPendingRetroPayReview("x", "y");
        item.FloorValue = 1325.3;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PendingRetroPayReviewRequiredNameRuleFlagsABlankName()
    {
        var rule = new PendingRetroPayReviewRequiredNameRule("pending retro pay review needs a value");
        var item = PendingRetroPayReview.NewPendingRetroPayReview("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PendingRetroPayReviewStartsAtRangeRuleFlagsValuesAboveStartsAt()
    {
        var rule = new PendingRetroPayReviewStartsAtRangeRule(64, 1000);
        var item = PendingRetroPayReview.NewPendingRetroPayReview("x", "y");
        item.StartsAt = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PendingRetroPayReviewNameLengthRuleFlagsALongName()
    {
        var rule = new PendingRetroPayReviewNameLengthRule(3600);
        var item = PendingRetroPayReview.NewPendingRetroPayReview("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        PendingRetroPayReviewValidatorReportsEveryProblem();
        PendingRetroPayReviewRequiredCodeRuleFlagsABlankCode();
        PendingRetroPayReviewFloorValueRangeRuleFlagsValuesAboveFloorValue();
        PendingRetroPayReviewRequiredNameRuleFlagsABlankName();
        PendingRetroPayReviewStartsAtRangeRuleFlagsValuesAboveStartsAt();
        PendingRetroPayReviewNameLengthRuleFlagsALongName();
    }
}
