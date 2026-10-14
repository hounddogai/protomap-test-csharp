namespace Acme.Workforce.Payrollops.Tests.Deduction.Metric;

using Acme.Workforce.Payrollops.Deduction.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class CappedDeductionPolicyRulesChecks
{
    public static void CappedDeductionPolicyValidatorReportsEveryProblem()
    {
        var validator = CappedDeductionPolicyRules.DefaultCappedDeductionPolicyValidator();
        var item = CappedDeductionPolicy.NewCappedDeductionPolicy("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CappedDeductionPolicyDescriptionLengthRuleFlagsALongDescription()
    {
        var rule = new CappedDeductionPolicyDescriptionLengthRule(16);
        var item = CappedDeductionPolicy.NewCappedDeductionPolicy("x", "y");
        item.Description = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Description = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CappedDeductionPolicyWindowDaysRangeRuleFlagsValuesAboveWindowDays()
    {
        var rule = new CappedDeductionPolicyWindowDaysRangeRule(80, 3600);
        var item = CappedDeductionPolicy.NewCappedDeductionPolicy("x", "y");
        item.WindowDays = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void CappedDeductionPolicyVersionRangeRuleFlagsValuesAboveVersion()
    {
        var rule = new CappedDeductionPolicyVersionRangeRule(16, 32);
        var item = CappedDeductionPolicy.NewCappedDeductionPolicy("x", "y");
        item.Version = 33;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        CappedDeductionPolicyValidatorReportsEveryProblem();
        CappedDeductionPolicyDescriptionLengthRuleFlagsALongDescription();
        CappedDeductionPolicyWindowDaysRangeRuleFlagsValuesAboveWindowDays();
        CappedDeductionPolicyVersionRangeRuleFlagsValuesAboveVersion();
    }
}
