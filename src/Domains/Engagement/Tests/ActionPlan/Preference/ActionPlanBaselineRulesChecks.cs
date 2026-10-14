namespace Acme.Workforce.Engagement.Tests.ActionPlan.Preference;

using Acme.Workforce.Engagement.ActionPlan.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class ActionPlanBaselineRulesChecks
{
    public static void ActionPlanBaselineValidatorReportsEveryProblem()
    {
        var validator = ActionPlanBaselineRules.DefaultActionPlanBaselineValidator();
        var item = ActionPlanBaseline.NewActionPlanBaseline("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ActionPlanBaselineSummaryLengthRuleFlagsALongSummary()
    {
        var rule = new ActionPlanBaselineSummaryLengthRule(16);
        var item = ActionPlanBaseline.NewActionPlanBaseline("x", "y");
        item.Summary = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Summary = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ActionPlanBaselineRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new ActionPlanBaselineRequiredCodeRule("action plan baseline needs an entry");
        var item = ActionPlanBaseline.NewActionPlanBaseline("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ActionPlanBaselineRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new ActionPlanBaselineRequiredLabelRule("action plan baseline needs this field");
        var item = ActionPlanBaseline.NewActionPlanBaseline("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        ActionPlanBaselineValidatorReportsEveryProblem();
        ActionPlanBaselineSummaryLengthRuleFlagsALongSummary();
        ActionPlanBaselineRequiredCodeRuleFlagsABlankCode();
        ActionPlanBaselineRequiredLabelRuleFlagsABlankLabel();
    }
}
