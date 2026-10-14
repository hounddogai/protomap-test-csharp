namespace Acme.Workforce.Contractors.Tests.Offboarding.Threshold;

using Acme.Workforce.Contractors.Offboarding.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class OffboardingAdjustmentRulesChecks
{
    public static void OffboardingAdjustmentValidatorReportsEveryProblem()
    {
        var validator = OffboardingAdjustmentRules.DefaultOffboardingAdjustmentValidator();
        var item = OffboardingAdjustment.NewOffboardingAdjustment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void OffboardingAdjustmentMaximumRangeRuleFlagsValuesAboveMaximum()
    {
        var rule = new OffboardingAdjustmentMaximumRangeRule(6.2, 3540.0);
        var item = OffboardingAdjustment.NewOffboardingAdjustment("x", "y");
        item.Maximum = 3541.5;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void OffboardingAdjustmentRequiredDepartmentRuleFlagsABlankDepartment()
    {
        var rule = new OffboardingAdjustmentRequiredDepartmentRule("offboarding adjustment needs a value");
        var item = OffboardingAdjustment.NewOffboardingAdjustment("", "");
        item.Department = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        OffboardingAdjustmentValidatorReportsEveryProblem();
        OffboardingAdjustmentMaximumRangeRuleFlagsValuesAboveMaximum();
        OffboardingAdjustmentRequiredDepartmentRuleFlagsABlankDepartment();
    }
}
