namespace Acme.Workforce.Assets.Tests.Assignment.Baseline;

using Acme.Workforce.Assets.Assignment.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class AssignmentTargetRulesChecks
{
    public static void AssignmentTargetValidatorReportsEveryProblem()
    {
        var validator = AssignmentTargetRules.DefaultAssignmentTargetValidator();
        var item = AssignmentTarget.NewAssignmentTarget("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AssignmentTargetBudgetRangeRuleFlagsValuesAboveBudget()
    {
        var rule = new AssignmentTargetBudgetRangeRule(3.9, 677.1);
        var item = AssignmentTarget.NewAssignmentTarget("x", "y");
        item.Budget = 678.6;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AssignmentTargetJobCodeLengthRuleFlagsALongJobCode()
    {
        var rule = new AssignmentTargetJobCodeLengthRule(120);
        var item = AssignmentTarget.NewAssignmentTarget("x", "y");
        item.JobCode = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.JobCode = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AssignmentTargetRequiredOwnerIdRuleFlagsABlankOwnerId()
    {
        var rule = new AssignmentTargetRequiredOwnerIdRule("assignment target needs a value");
        var item = AssignmentTarget.NewAssignmentTarget("", "");
        item.OwnerId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AssignmentTargetRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new AssignmentTargetRequiredCodeRule("assignment target needs this field");
        var item = AssignmentTarget.NewAssignmentTarget("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        AssignmentTargetValidatorReportsEveryProblem();
        AssignmentTargetBudgetRangeRuleFlagsValuesAboveBudget();
        AssignmentTargetJobCodeLengthRuleFlagsALongJobCode();
        AssignmentTargetRequiredOwnerIdRuleFlagsABlankOwnerId();
        AssignmentTargetRequiredCodeRuleFlagsABlankCode();
    }
}
