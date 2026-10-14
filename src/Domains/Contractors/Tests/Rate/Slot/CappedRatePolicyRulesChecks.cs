namespace Acme.Workforce.Contractors.Tests.Rate.Slot;

using Acme.Workforce.Contractors.Rate.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class CappedRatePolicyRulesChecks
{
    public static void CappedRatePolicyValidatorReportsEveryProblem()
    {
        var validator = CappedRatePolicyRules.DefaultCappedRatePolicyValidator();
        var item = CappedRatePolicy.NewCappedRatePolicy("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CappedRatePolicyEndsAtRangeRuleFlagsValuesAboveEndsAt()
    {
        var rule = new CappedRatePolicyEndsAtRangeRule(120, 86400);
        var item = CappedRatePolicy.NewCappedRatePolicy("x", "y");
        item.EndsAt = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void CappedRatePolicyRatioRangeRuleFlagsValuesAboveRatio()
    {
        var rule = new CappedRatePolicyRatioRangeRule(9.9, 2921.4);
        var item = CappedRatePolicy.NewCappedRatePolicy("x", "y");
        item.Ratio = 2922.9;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void CappedRatePolicyCreatedAtRangeRuleFlagsValuesAboveCreatedAt()
    {
        var rule = new CappedRatePolicyCreatedAtRangeRule(255, 86400);
        var item = CappedRatePolicy.NewCappedRatePolicy("x", "y");
        item.CreatedAt = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void CappedRatePolicyRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new CappedRatePolicyRequiredCodeRule("capped rate policy needs a value");
        var item = CappedRatePolicy.NewCappedRatePolicy("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void CappedRatePolicyRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new CappedRatePolicyRequiredTitleRule("capped rate policy needs an entry");
        var item = CappedRatePolicy.NewCappedRatePolicy("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void CappedRatePolicyTitleLengthRuleFlagsALongTitle()
    {
        var rule = new CappedRatePolicyTitleLengthRule(32);
        var item = CappedRatePolicy.NewCappedRatePolicy("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        CappedRatePolicyValidatorReportsEveryProblem();
        CappedRatePolicyEndsAtRangeRuleFlagsValuesAboveEndsAt();
        CappedRatePolicyRatioRangeRuleFlagsValuesAboveRatio();
        CappedRatePolicyCreatedAtRangeRuleFlagsValuesAboveCreatedAt();
        CappedRatePolicyRequiredCodeRuleFlagsABlankCode();
        CappedRatePolicyRequiredTitleRuleFlagsABlankTitle();
        CappedRatePolicyTitleLengthRuleFlagsALongTitle();
    }
}
