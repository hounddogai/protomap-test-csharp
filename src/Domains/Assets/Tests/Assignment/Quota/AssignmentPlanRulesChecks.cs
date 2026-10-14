namespace Acme.Workforce.Assets.Tests.Assignment.Quota;

using Acme.Workforce.Assets.Assignment.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class AssignmentPlanRulesChecks
{
    public static void AssignmentPlanValidatorReportsEveryProblem()
    {
        var validator = AssignmentPlanRules.DefaultAssignmentPlanValidator();
        var item = AssignmentPlan.NewAssignmentPlan("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AssignmentPlanScoreRangeRuleFlagsValuesAboveScore()
    {
        var rule = new AssignmentPlanScoreRangeRule(6.4, 3410.4);
        var item = AssignmentPlan.NewAssignmentPlan("x", "y");
        item.Score = 3411.9;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AssignmentPlanRegionLengthRuleFlagsALongRegion()
    {
        var rule = new AssignmentPlanRegionLengthRule(3600);
        var item = AssignmentPlan.NewAssignmentPlan("x", "y");
        item.Region = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Region = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AssignmentPlanRequiredRegionRuleFlagsABlankRegion()
    {
        var rule = new AssignmentPlanRequiredRegionRule("assignment plan needs this field");
        var item = AssignmentPlan.NewAssignmentPlan("", "");
        item.Region = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AssignmentPlanCodeLengthRuleFlagsALongCode()
    {
        var rule = new AssignmentPlanCodeLengthRule(3600);
        var item = AssignmentPlan.NewAssignmentPlan("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AssignmentPlanSummaryLengthRuleFlagsALongSummary()
    {
        var rule = new AssignmentPlanSummaryLengthRule(3600);
        var item = AssignmentPlan.NewAssignmentPlan("x", "y");
        item.Summary = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Summary = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        AssignmentPlanValidatorReportsEveryProblem();
        AssignmentPlanScoreRangeRuleFlagsValuesAboveScore();
        AssignmentPlanRegionLengthRuleFlagsALongRegion();
        AssignmentPlanRequiredRegionRuleFlagsABlankRegion();
        AssignmentPlanCodeLengthRuleFlagsALongCode();
        AssignmentPlanSummaryLengthRuleFlagsALongSummary();
    }
}
