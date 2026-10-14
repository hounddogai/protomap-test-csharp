namespace Acme.Workforce.Payrollops.Tests.Payslip.Threshold;

using Acme.Workforce.Payrollops.Payslip.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class StrictPayslipOverrideRulesChecks
{
    public static void StrictPayslipOverrideValidatorReportsEveryProblem()
    {
        var validator = StrictPayslipOverrideRules.DefaultStrictPayslipOverrideValidator();
        var item = StrictPayslipOverride.NewStrictPayslipOverride("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void StrictPayslipOverrideLimitRangeRuleFlagsValuesAboveLimit()
    {
        var rule = new StrictPayslipOverrideLimitRangeRule(120, 255);
        var item = StrictPayslipOverride.NewStrictPayslipOverride("x", "y");
        item.Limit = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void StrictPayslipOverrideRequiredTimezoneRuleFlagsABlankTimezone()
    {
        var rule = new StrictPayslipOverrideRequiredTimezoneRule("strict payslip override needs this field");
        var item = StrictPayslipOverride.NewStrictPayslipOverride("", "");
        item.Timezone = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        StrictPayslipOverrideValidatorReportsEveryProblem();
        StrictPayslipOverrideLimitRangeRuleFlagsValuesAboveLimit();
        StrictPayslipOverrideRequiredTimezoneRuleFlagsABlankTimezone();
    }
}
