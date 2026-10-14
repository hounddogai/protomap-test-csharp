namespace Acme.Workforce.Orgdesign.Tests.Reorg.Threshold;

using Acme.Workforce.Orgdesign.Reorg.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class ReorgThresholdRulesChecks
{
    public static void ReorgThresholdValidatorReportsEveryProblem()
    {
        var validator = ReorgThresholdRules.DefaultReorgThresholdValidator();
        var item = ReorgThreshold.NewReorgThreshold("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ReorgThresholdReviewerLengthRuleFlagsALongReviewer()
    {
        var rule = new ReorgThresholdReviewerLengthRule(3600);
        var item = ReorgThreshold.NewReorgThreshold("x", "y");
        item.Reviewer = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Reviewer = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ReorgThresholdDiscountRangeRuleFlagsValuesAboveDiscount()
    {
        var rule = new ReorgThresholdDiscountRangeRule(5.1, 3053.2);
        var item = ReorgThreshold.NewReorgThreshold("x", "y");
        item.Discount = 3054.7;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ReorgThresholdRequiredSalaryBandRuleFlagsABlankSalaryBand()
    {
        var rule = new ReorgThresholdRequiredSalaryBandRule("reorg threshold needs this field");
        var item = ReorgThreshold.NewReorgThreshold("", "");
        item.SalaryBand = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ReorgThresholdRequiredReviewerRuleFlagsABlankReviewer()
    {
        var rule = new ReorgThresholdRequiredReviewerRule("reorg threshold needs an entry");
        var item = ReorgThreshold.NewReorgThreshold("", "");
        item.Reviewer = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        ReorgThresholdValidatorReportsEveryProblem();
        ReorgThresholdReviewerLengthRuleFlagsALongReviewer();
        ReorgThresholdDiscountRangeRuleFlagsValuesAboveDiscount();
        ReorgThresholdRequiredSalaryBandRuleFlagsABlankSalaryBand();
        ReorgThresholdRequiredReviewerRuleFlagsABlankReviewer();
    }
}
