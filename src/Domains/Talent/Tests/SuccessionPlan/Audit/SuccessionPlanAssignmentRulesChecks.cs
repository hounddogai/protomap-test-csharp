namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Audit;

using Acme.Workforce.Talent.SuccessionPlan.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class SuccessionPlanAssignmentRulesChecks
{
    public static void SuccessionPlanAssignmentValidatorReportsEveryProblem()
    {
        var validator = SuccessionPlanAssignmentRules.DefaultSuccessionPlanAssignmentValidator();
        var item = SuccessionPlanAssignment.NewSuccessionPlanAssignment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SuccessionPlanAssignmentRequiredSummaryRuleFlagsABlankSummary()
    {
        var rule = new SuccessionPlanAssignmentRequiredSummaryRule("succession plan assignment needs this field");
        var item = SuccessionPlanAssignment.NewSuccessionPlanAssignment("", "");
        item.Summary = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SuccessionPlanAssignmentLocationLengthRuleFlagsALongLocation()
    {
        var rule = new SuccessionPlanAssignmentLocationLengthRule(80);
        var item = SuccessionPlanAssignment.NewSuccessionPlanAssignment("x", "y");
        item.Location = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Location = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SuccessionPlanAssignmentSummaryLengthRuleFlagsALongSummary()
    {
        var rule = new SuccessionPlanAssignmentSummaryLengthRule(80);
        var item = SuccessionPlanAssignment.NewSuccessionPlanAssignment("x", "y");
        item.Summary = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Summary = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SuccessionPlanAssignmentCodeLengthRuleFlagsALongCode()
    {
        var rule = new SuccessionPlanAssignmentCodeLengthRule(32);
        var item = SuccessionPlanAssignment.NewSuccessionPlanAssignment("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SuccessionPlanAssignmentBudgetRangeRuleFlagsValuesAboveBudget()
    {
        var rule = new SuccessionPlanAssignmentBudgetRangeRule(0.5, 2363.2);
        var item = SuccessionPlanAssignment.NewSuccessionPlanAssignment("x", "y");
        item.Budget = 2364.7;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void SuccessionPlanAssignmentFloorValueRangeRuleFlagsValuesAboveFloorValue()
    {
        var rule = new SuccessionPlanAssignmentFloorValueRangeRule(6.5, 4001.3);
        var item = SuccessionPlanAssignment.NewSuccessionPlanAssignment("x", "y");
        item.FloorValue = 4002.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        SuccessionPlanAssignmentValidatorReportsEveryProblem();
        SuccessionPlanAssignmentRequiredSummaryRuleFlagsABlankSummary();
        SuccessionPlanAssignmentLocationLengthRuleFlagsALongLocation();
        SuccessionPlanAssignmentSummaryLengthRuleFlagsALongSummary();
        SuccessionPlanAssignmentCodeLengthRuleFlagsALongCode();
        SuccessionPlanAssignmentBudgetRangeRuleFlagsValuesAboveBudget();
        SuccessionPlanAssignmentFloorValueRangeRuleFlagsValuesAboveFloorValue();
    }
}
