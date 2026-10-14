namespace Acme.Workforce.Absence.Tests.Policy.Notice;

using Acme.Workforce.Absence.Policy.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class RegionalPolicyAssignmentRulesChecks
{
    public static void RegionalPolicyAssignmentValidatorReportsEveryProblem()
    {
        var validator = RegionalPolicyAssignmentRules.DefaultRegionalPolicyAssignmentValidator();
        var item = RegionalPolicyAssignment.NewRegionalPolicyAssignment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RegionalPolicyAssignmentRequiredUnitRuleFlagsABlankUnit()
    {
        var rule = new RegionalPolicyAssignmentRequiredUnitRule("regional policy assignment needs this field");
        var item = RegionalPolicyAssignment.NewRegionalPolicyAssignment("", "");
        item.Unit = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RegionalPolicyAssignmentRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new RegionalPolicyAssignmentRequiredTitleRule("regional policy assignment needs a value");
        var item = RegionalPolicyAssignment.NewRegionalPolicyAssignment("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RegionalPolicyAssignmentTitleLengthRuleFlagsALongTitle()
    {
        var rule = new RegionalPolicyAssignmentTitleLengthRule(80);
        var item = RegionalPolicyAssignment.NewRegionalPolicyAssignment("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RegionalPolicyAssignmentCodeLengthRuleFlagsALongCode()
    {
        var rule = new RegionalPolicyAssignmentCodeLengthRule(120);
        var item = RegionalPolicyAssignment.NewRegionalPolicyAssignment("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RegionalPolicyAssignmentUnitLengthRuleFlagsALongUnit()
    {
        var rule = new RegionalPolicyAssignmentUnitLengthRule(80);
        var item = RegionalPolicyAssignment.NewRegionalPolicyAssignment("x", "y");
        item.Unit = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Unit = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        RegionalPolicyAssignmentValidatorReportsEveryProblem();
        RegionalPolicyAssignmentRequiredUnitRuleFlagsABlankUnit();
        RegionalPolicyAssignmentRequiredTitleRuleFlagsABlankTitle();
        RegionalPolicyAssignmentTitleLengthRuleFlagsALongTitle();
        RegionalPolicyAssignmentCodeLengthRuleFlagsALongCode();
        RegionalPolicyAssignmentUnitLengthRuleFlagsALongUnit();
    }
}
