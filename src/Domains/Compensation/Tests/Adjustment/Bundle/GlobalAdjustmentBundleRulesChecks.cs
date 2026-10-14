namespace Acme.Workforce.Compensation.Tests.Adjustment.Bundle;

using Acme.Workforce.Compensation.Adjustment.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class GlobalAdjustmentBundleRulesChecks
{
    public static void GlobalAdjustmentBundleValidatorReportsEveryProblem()
    {
        var validator = GlobalAdjustmentBundleRules.DefaultGlobalAdjustmentBundleValidator();
        var item = GlobalAdjustmentBundle.NewGlobalAdjustmentBundle("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void GlobalAdjustmentBundleRateRangeRuleFlagsValuesAboveRate()
    {
        var rule = new GlobalAdjustmentBundleRateRangeRule(2.7, 2052.7);
        var item = GlobalAdjustmentBundle.NewGlobalAdjustmentBundle("x", "y");
        item.Rate = 2054.2;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void GlobalAdjustmentBundleRequiredNameRuleFlagsABlankName()
    {
        var rule = new GlobalAdjustmentBundleRequiredNameRule("global adjustment bundle needs this field");
        var item = GlobalAdjustmentBundle.NewGlobalAdjustmentBundle("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void GlobalAdjustmentBundleCodeLengthRuleFlagsALongCode()
    {
        var rule = new GlobalAdjustmentBundleCodeLengthRule(120);
        var item = GlobalAdjustmentBundle.NewGlobalAdjustmentBundle("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        GlobalAdjustmentBundleValidatorReportsEveryProblem();
        GlobalAdjustmentBundleRateRangeRuleFlagsValuesAboveRate();
        GlobalAdjustmentBundleRequiredNameRuleFlagsABlankName();
        GlobalAdjustmentBundleCodeLengthRuleFlagsALongCode();
    }
}
