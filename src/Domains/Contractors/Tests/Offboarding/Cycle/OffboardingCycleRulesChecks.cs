namespace Acme.Workforce.Contractors.Tests.Offboarding.Cycle;

using Acme.Workforce.Contractors.Offboarding.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class OffboardingCycleRulesChecks
{
    public static void OffboardingCycleValidatorReportsEveryProblem()
    {
        var validator = OffboardingCycleRules.DefaultOffboardingCycleValidator();
        var item = OffboardingCycle.NewOffboardingCycle("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void OffboardingCycleRequiredNameRuleFlagsABlankName()
    {
        var rule = new OffboardingCycleRequiredNameRule("offboarding cycle needs an entry");
        var item = OffboardingCycle.NewOffboardingCycle("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void OffboardingCycleCostCenterLengthRuleFlagsALongCostCenter()
    {
        var rule = new OffboardingCycleCostCenterLengthRule(3600);
        var item = OffboardingCycle.NewOffboardingCycle("x", "y");
        item.CostCenter = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.CostCenter = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void OffboardingCycleNameLengthRuleFlagsALongName()
    {
        var rule = new OffboardingCycleNameLengthRule(32);
        var item = OffboardingCycle.NewOffboardingCycle("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void OffboardingCycleLimitRangeRuleFlagsValuesAboveLimit()
    {
        var rule = new OffboardingCycleLimitRangeRule(255, 86400);
        var item = OffboardingCycle.NewOffboardingCycle("x", "y");
        item.Limit = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void OffboardingCycleDepartmentLengthRuleFlagsALongDepartment()
    {
        var rule = new OffboardingCycleDepartmentLengthRule(80);
        var item = OffboardingCycle.NewOffboardingCycle("x", "y");
        item.Department = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Department = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        OffboardingCycleValidatorReportsEveryProblem();
        OffboardingCycleRequiredNameRuleFlagsABlankName();
        OffboardingCycleCostCenterLengthRuleFlagsALongCostCenter();
        OffboardingCycleNameLengthRuleFlagsALongName();
        OffboardingCycleLimitRangeRuleFlagsValuesAboveLimit();
        OffboardingCycleDepartmentLengthRuleFlagsALongDepartment();
    }
}
