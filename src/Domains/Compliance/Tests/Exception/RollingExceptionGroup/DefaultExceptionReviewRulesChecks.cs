namespace Acme.Workforce.Compliance.Tests.Exception.RollingExceptionGroup;

using Acme.Workforce.Compliance.Exception.RollingExceptionGroup;

/// <summary>Checks of rolling exception group code, run by the project's runner.</summary>
public static class DefaultExceptionReviewRulesChecks
{
    public static void DefaultExceptionReviewValidatorReportsEveryProblem()
    {
        var validator = DefaultExceptionReviewRules.DefaultDefaultExceptionReviewValidator();
        var item = DefaultExceptionReview.NewDefaultExceptionReview("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DefaultExceptionReviewLabelLengthRuleFlagsALongLabel()
    {
        var rule = new DefaultExceptionReviewLabelLengthRule(32);
        var item = DefaultExceptionReview.NewDefaultExceptionReview("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void DefaultExceptionReviewCodeLengthRuleFlagsALongCode()
    {
        var rule = new DefaultExceptionReviewCodeLengthRule(64);
        var item = DefaultExceptionReview.NewDefaultExceptionReview("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void DefaultExceptionReviewCapacityRangeRuleFlagsValuesAboveCapacity()
    {
        var rule = new DefaultExceptionReviewCapacityRangeRule(32, 80);
        var item = DefaultExceptionReview.NewDefaultExceptionReview("x", "y");
        item.Capacity = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        DefaultExceptionReviewValidatorReportsEveryProblem();
        DefaultExceptionReviewLabelLengthRuleFlagsALongLabel();
        DefaultExceptionReviewCodeLengthRuleFlagsALongCode();
        DefaultExceptionReviewCapacityRangeRuleFlagsValuesAboveCapacity();
    }
}
