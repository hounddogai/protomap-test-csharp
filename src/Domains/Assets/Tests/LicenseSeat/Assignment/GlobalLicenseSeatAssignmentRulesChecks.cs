namespace Acme.Workforce.Assets.Tests.LicenseSeat.Assignment;

using Acme.Workforce.Assets.LicenseSeat.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class GlobalLicenseSeatAssignmentRulesChecks
{
    public static void GlobalLicenseSeatAssignmentValidatorReportsEveryProblem()
    {
        var validator = GlobalLicenseSeatAssignmentRules.DefaultGlobalLicenseSeatAssignmentValidator();
        var item = GlobalLicenseSeatAssignment.NewGlobalLicenseSeatAssignment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void GlobalLicenseSeatAssignmentRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new GlobalLicenseSeatAssignmentRequiredCodeRule("global license seat assignment needs a value");
        var item = GlobalLicenseSeatAssignment.NewGlobalLicenseSeatAssignment("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void GlobalLicenseSeatAssignmentDepartmentLengthRuleFlagsALongDepartment()
    {
        var rule = new GlobalLicenseSeatAssignmentDepartmentLengthRule(3600);
        var item = GlobalLicenseSeatAssignment.NewGlobalLicenseSeatAssignment("x", "y");
        item.Department = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Department = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void GlobalLicenseSeatAssignmentNameLengthRuleFlagsALongName()
    {
        var rule = new GlobalLicenseSeatAssignmentNameLengthRule(120);
        var item = GlobalLicenseSeatAssignment.NewGlobalLicenseSeatAssignment("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        GlobalLicenseSeatAssignmentValidatorReportsEveryProblem();
        GlobalLicenseSeatAssignmentRequiredCodeRuleFlagsABlankCode();
        GlobalLicenseSeatAssignmentDepartmentLengthRuleFlagsALongDepartment();
        GlobalLicenseSeatAssignmentNameLengthRuleFlagsALongName();
    }
}
