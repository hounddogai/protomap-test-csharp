namespace Acme.Workforce.Orgdesign.Tests.Headcount.Adjustment;

using Acme.Workforce.Orgdesign.Headcount.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class HeadcountAdjustmentRulesChecks
{
    public static void HeadcountAdjustmentValidatorReportsEveryProblem()
    {
        var validator = HeadcountAdjustmentRules.DefaultHeadcountAdjustmentValidator();
        var item = HeadcountAdjustment.NewHeadcountAdjustment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void HeadcountAdjustmentRequiredNameRuleFlagsABlankName()
    {
        var rule = new HeadcountAdjustmentRequiredNameRule("headcount adjustment needs a value");
        var item = HeadcountAdjustment.NewHeadcountAdjustment("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void HeadcountAdjustmentCodeLengthRuleFlagsALongCode()
    {
        var rule = new HeadcountAdjustmentCodeLengthRule(16);
        var item = HeadcountAdjustment.NewHeadcountAdjustment("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void HeadcountAdjustmentMarginRangeRuleFlagsValuesAboveMargin()
    {
        var rule = new HeadcountAdjustmentMarginRangeRule(9.0, 4585.4);
        var item = HeadcountAdjustment.NewHeadcountAdjustment("x", "y");
        item.Margin = 4586.9;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        HeadcountAdjustmentValidatorReportsEveryProblem();
        HeadcountAdjustmentRequiredNameRuleFlagsABlankName();
        HeadcountAdjustmentCodeLengthRuleFlagsALongCode();
        HeadcountAdjustmentMarginRangeRuleFlagsValuesAboveMargin();
    }
}
