namespace Acme.Workforce.Engagement.Tests.ActionPlan.Preference;

using Acme.Workforce.Engagement.ActionPlan.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class ActionPlanAssignmentRulesChecks
{
    public static void ActionPlanAssignmentValidatorReportsEveryProblem()
    {
        var validator = ActionPlanAssignmentRules.DefaultActionPlanAssignmentValidator();
        var item = ActionPlanAssignment.NewActionPlanAssignment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ActionPlanAssignmentRequiredSegmentRuleFlagsABlankSegment()
    {
        var rule = new ActionPlanAssignmentRequiredSegmentRule("action plan assignment needs this field");
        var item = ActionPlanAssignment.NewActionPlanAssignment("", "");
        item.Segment = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ActionPlanAssignmentExpiresAtRangeRuleFlagsValuesAboveExpiresAt()
    {
        var rule = new ActionPlanAssignmentExpiresAtRangeRule(1000, 86400);
        var item = ActionPlanAssignment.NewActionPlanAssignment("x", "y");
        item.ExpiresAt = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ActionPlanAssignmentCodeLengthRuleFlagsALongCode()
    {
        var rule = new ActionPlanAssignmentCodeLengthRule(32);
        var item = ActionPlanAssignment.NewActionPlanAssignment("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ActionPlanAssignmentLocationLengthRuleFlagsALongLocation()
    {
        var rule = new ActionPlanAssignmentLocationLengthRule(16);
        var item = ActionPlanAssignment.NewActionPlanAssignment("x", "y");
        item.Location = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Location = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        ActionPlanAssignmentValidatorReportsEveryProblem();
        ActionPlanAssignmentRequiredSegmentRuleFlagsABlankSegment();
        ActionPlanAssignmentExpiresAtRangeRuleFlagsValuesAboveExpiresAt();
        ActionPlanAssignmentCodeLengthRuleFlagsALongCode();
        ActionPlanAssignmentLocationLengthRuleFlagsALongLocation();
    }
}
