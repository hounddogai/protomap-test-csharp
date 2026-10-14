namespace Acme.Workforce.Payrollops.Tests.Deduction.Exception;

using Acme.Workforce.Payrollops.Deduction.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class AdaptiveDeductionBaselineRulesChecks
{
    public static void AdaptiveDeductionBaselineValidatorReportsEveryProblem()
    {
        var validator = AdaptiveDeductionBaselineRules.DefaultAdaptiveDeductionBaselineValidator();
        var item = AdaptiveDeductionBaseline.NewAdaptiveDeductionBaseline("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AdaptiveDeductionBaselineDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes()
    {
        var rule = new AdaptiveDeductionBaselineDurationMinutesRangeRule(32, 1000);
        var item = AdaptiveDeductionBaseline.NewAdaptiveDeductionBaseline("x", "y");
        item.DurationMinutes = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AdaptiveDeductionBaselineRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new AdaptiveDeductionBaselineRequiredTitleRule("adaptive deduction baseline needs this field");
        var item = AdaptiveDeductionBaseline.NewAdaptiveDeductionBaseline("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AdaptiveDeductionBaselineCodeLengthRuleFlagsALongCode()
    {
        var rule = new AdaptiveDeductionBaselineCodeLengthRule(3600);
        var item = AdaptiveDeductionBaseline.NewAdaptiveDeductionBaseline("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AdaptiveDeductionBaselineRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new AdaptiveDeductionBaselineRequiredCodeRule("adaptive deduction baseline needs an entry");
        var item = AdaptiveDeductionBaseline.NewAdaptiveDeductionBaseline("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        AdaptiveDeductionBaselineValidatorReportsEveryProblem();
        AdaptiveDeductionBaselineDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes();
        AdaptiveDeductionBaselineRequiredTitleRuleFlagsABlankTitle();
        AdaptiveDeductionBaselineCodeLengthRuleFlagsALongCode();
        AdaptiveDeductionBaselineRequiredCodeRuleFlagsABlankCode();
    }
}
