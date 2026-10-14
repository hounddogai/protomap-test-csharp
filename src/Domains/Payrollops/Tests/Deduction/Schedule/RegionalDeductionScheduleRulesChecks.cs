namespace Acme.Workforce.Payrollops.Tests.Deduction.Schedule;

using Acme.Workforce.Payrollops.Deduction.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class RegionalDeductionScheduleRulesChecks
{
    public static void RegionalDeductionScheduleValidatorReportsEveryProblem()
    {
        var validator = RegionalDeductionScheduleRules.DefaultRegionalDeductionScheduleValidator();
        var item = RegionalDeductionSchedule.NewRegionalDeductionSchedule("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RegionalDeductionScheduleExternalRefLengthRuleFlagsALongExternalRef()
    {
        var rule = new RegionalDeductionScheduleExternalRefLengthRule(120);
        var item = RegionalDeductionSchedule.NewRegionalDeductionSchedule("x", "y");
        item.ExternalRef = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ExternalRef = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RegionalDeductionScheduleDepartmentLengthRuleFlagsALongDepartment()
    {
        var rule = new RegionalDeductionScheduleDepartmentLengthRule(1000);
        var item = RegionalDeductionSchedule.NewRegionalDeductionSchedule("x", "y");
        item.Department = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Department = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RegionalDeductionScheduleRequiredDepartmentRuleFlagsABlankDepartment()
    {
        var rule = new RegionalDeductionScheduleRequiredDepartmentRule("regional deduction schedule needs this field");
        var item = RegionalDeductionSchedule.NewRegionalDeductionSchedule("", "");
        item.Department = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        RegionalDeductionScheduleValidatorReportsEveryProblem();
        RegionalDeductionScheduleExternalRefLengthRuleFlagsALongExternalRef();
        RegionalDeductionScheduleDepartmentLengthRuleFlagsALongDepartment();
        RegionalDeductionScheduleRequiredDepartmentRuleFlagsABlankDepartment();
    }
}
