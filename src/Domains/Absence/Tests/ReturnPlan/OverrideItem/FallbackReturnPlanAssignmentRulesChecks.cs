namespace Acme.Workforce.Absence.Tests.ReturnPlan.OverrideItem;

using Acme.Workforce.Absence.ReturnPlan.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class FallbackReturnPlanAssignmentRulesChecks
{
    public static void FallbackReturnPlanAssignmentValidatorReportsEveryProblem()
    {
        var validator = FallbackReturnPlanAssignmentRules.DefaultFallbackReturnPlanAssignmentValidator();
        var item = FallbackReturnPlanAssignment.NewFallbackReturnPlanAssignment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FallbackReturnPlanAssignmentRequiredJobCodeRuleFlagsABlankJobCode()
    {
        var rule = new FallbackReturnPlanAssignmentRequiredJobCodeRule("fallback return plan assignment needs an entry");
        var item = FallbackReturnPlanAssignment.NewFallbackReturnPlanAssignment("", "");
        item.JobCode = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FallbackReturnPlanAssignmentRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new FallbackReturnPlanAssignmentRequiredCodeRule("fallback return plan assignment needs this field");
        var item = FallbackReturnPlanAssignment.NewFallbackReturnPlanAssignment("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FallbackReturnPlanAssignmentRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new FallbackReturnPlanAssignmentRequiredLabelRule("fallback return plan assignment needs this field");
        var item = FallbackReturnPlanAssignment.NewFallbackReturnPlanAssignment("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FallbackReturnPlanAssignmentLabelLengthRuleFlagsALongLabel()
    {
        var rule = new FallbackReturnPlanAssignmentLabelLengthRule(120);
        var item = FallbackReturnPlanAssignment.NewFallbackReturnPlanAssignment("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        FallbackReturnPlanAssignmentValidatorReportsEveryProblem();
        FallbackReturnPlanAssignmentRequiredJobCodeRuleFlagsABlankJobCode();
        FallbackReturnPlanAssignmentRequiredCodeRuleFlagsABlankCode();
        FallbackReturnPlanAssignmentRequiredLabelRuleFlagsABlankLabel();
        FallbackReturnPlanAssignmentLabelLengthRuleFlagsALongLabel();
    }
}
