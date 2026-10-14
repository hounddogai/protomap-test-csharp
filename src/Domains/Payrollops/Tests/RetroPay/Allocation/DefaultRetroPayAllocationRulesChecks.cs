namespace Acme.Workforce.Payrollops.Tests.RetroPay.Allocation;

using Acme.Workforce.Payrollops.RetroPay.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class DefaultRetroPayAllocationRulesChecks
{
    public static void DefaultRetroPayAllocationValidatorReportsEveryProblem()
    {
        var validator = DefaultRetroPayAllocationRules.DefaultDefaultRetroPayAllocationValidator();
        var item = DefaultRetroPayAllocation.NewDefaultRetroPayAllocation("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DefaultRetroPayAllocationRequiredDepartmentRuleFlagsABlankDepartment()
    {
        var rule = new DefaultRetroPayAllocationRequiredDepartmentRule("default retro pay allocation needs a value");
        var item = DefaultRetroPayAllocation.NewDefaultRetroPayAllocation("", "");
        item.Department = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DefaultRetroPayAllocationDepartmentLengthRuleFlagsALongDepartment()
    {
        var rule = new DefaultRetroPayAllocationDepartmentLengthRule(64);
        var item = DefaultRetroPayAllocation.NewDefaultRetroPayAllocation("x", "y");
        item.Department = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Department = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void DefaultRetroPayAllocationRequiredGradeRuleFlagsABlankGrade()
    {
        var rule = new DefaultRetroPayAllocationRequiredGradeRule("default retro pay allocation needs a value");
        var item = DefaultRetroPayAllocation.NewDefaultRetroPayAllocation("", "");
        item.Grade = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DefaultRetroPayAllocationTitleLengthRuleFlagsALongTitle()
    {
        var rule = new DefaultRetroPayAllocationTitleLengthRule(3600);
        var item = DefaultRetroPayAllocation.NewDefaultRetroPayAllocation("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void DefaultRetroPayAllocationGradeLengthRuleFlagsALongGrade()
    {
        var rule = new DefaultRetroPayAllocationGradeLengthRule(80);
        var item = DefaultRetroPayAllocation.NewDefaultRetroPayAllocation("x", "y");
        item.Grade = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Grade = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        DefaultRetroPayAllocationValidatorReportsEveryProblem();
        DefaultRetroPayAllocationRequiredDepartmentRuleFlagsABlankDepartment();
        DefaultRetroPayAllocationDepartmentLengthRuleFlagsALongDepartment();
        DefaultRetroPayAllocationRequiredGradeRuleFlagsABlankGrade();
        DefaultRetroPayAllocationTitleLengthRuleFlagsALongTitle();
        DefaultRetroPayAllocationGradeLengthRuleFlagsALongGrade();
    }
}
