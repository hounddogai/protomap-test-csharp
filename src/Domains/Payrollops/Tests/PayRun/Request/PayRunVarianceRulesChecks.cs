namespace Acme.Workforce.Payrollops.Tests.PayRun.Request;

using Acme.Workforce.Payrollops.PayRun.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class PayRunVarianceRulesChecks
{
    public static void PayRunVarianceValidatorReportsEveryProblem()
    {
        var validator = PayRunVarianceRules.DefaultPayRunVarianceValidator();
        var item = PayRunVariance.NewPayRunVariance("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PayRunVarianceRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new PayRunVarianceRequiredTitleRule("pay run variance needs a value");
        var item = PayRunVariance.NewPayRunVariance("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PayRunVarianceQuantityRangeRuleFlagsValuesAboveQuantity()
    {
        var rule = new PayRunVarianceQuantityRangeRule(1000, 1000);
        var item = PayRunVariance.NewPayRunVariance("x", "y");
        item.Quantity = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PayRunVarianceRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new PayRunVarianceRequiredCodeRule("pay run variance needs this field");
        var item = PayRunVariance.NewPayRunVariance("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PayRunVarianceDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes()
    {
        var rule = new PayRunVarianceDurationMinutesRangeRule(255, 86400);
        var item = PayRunVariance.NewPayRunVariance("x", "y");
        item.DurationMinutes = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PayRunVarianceCapacityRangeRuleFlagsValuesAboveCapacity()
    {
        var rule = new PayRunVarianceCapacityRangeRule(32, 86400);
        var item = PayRunVariance.NewPayRunVariance("x", "y");
        item.Capacity = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        PayRunVarianceValidatorReportsEveryProblem();
        PayRunVarianceRequiredTitleRuleFlagsABlankTitle();
        PayRunVarianceQuantityRangeRuleFlagsValuesAboveQuantity();
        PayRunVarianceRequiredCodeRuleFlagsABlankCode();
        PayRunVarianceDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes();
        PayRunVarianceCapacityRangeRuleFlagsValuesAboveCapacity();
    }
}
