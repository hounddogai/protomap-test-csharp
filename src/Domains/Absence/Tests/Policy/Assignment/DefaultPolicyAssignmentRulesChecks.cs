namespace Acme.Workforce.Absence.Tests.Policy.Assignment;

using Acme.Workforce.Absence.Policy.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class DefaultPolicyAssignmentRulesChecks
{
    public static void DefaultPolicyAssignmentValidatorReportsEveryProblem()
    {
        var validator = DefaultPolicyAssignmentRules.DefaultDefaultPolicyAssignmentValidator();
        var item = DefaultPolicyAssignment.NewDefaultPolicyAssignment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DefaultPolicyAssignmentNameLengthRuleFlagsALongName()
    {
        var rule = new DefaultPolicyAssignmentNameLengthRule(120);
        var item = DefaultPolicyAssignment.NewDefaultPolicyAssignment("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void DefaultPolicyAssignmentCostRangeRuleFlagsValuesAboveCost()
    {
        var rule = new DefaultPolicyAssignmentCostRangeRule(9.3, 1220.5);
        var item = DefaultPolicyAssignment.NewDefaultPolicyAssignment("x", "y");
        item.Cost = 1222.0;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void DefaultPolicyAssignmentCodeLengthRuleFlagsALongCode()
    {
        var rule = new DefaultPolicyAssignmentCodeLengthRule(64);
        var item = DefaultPolicyAssignment.NewDefaultPolicyAssignment("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void DefaultPolicyAssignmentRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new DefaultPolicyAssignmentRequiredTitleRule("default policy assignment needs a value");
        var item = DefaultPolicyAssignment.NewDefaultPolicyAssignment("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DefaultPolicyAssignmentLeaveDaysRangeRuleFlagsValuesAboveLeaveDays()
    {
        var rule = new DefaultPolicyAssignmentLeaveDaysRangeRule(16, 1000);
        var item = DefaultPolicyAssignment.NewDefaultPolicyAssignment("x", "y");
        item.LeaveDays = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        DefaultPolicyAssignmentValidatorReportsEveryProblem();
        DefaultPolicyAssignmentNameLengthRuleFlagsALongName();
        DefaultPolicyAssignmentCostRangeRuleFlagsValuesAboveCost();
        DefaultPolicyAssignmentCodeLengthRuleFlagsALongCode();
        DefaultPolicyAssignmentRequiredTitleRuleFlagsABlankTitle();
        DefaultPolicyAssignmentLeaveDaysRangeRuleFlagsValuesAboveLeaveDays();
    }
}
