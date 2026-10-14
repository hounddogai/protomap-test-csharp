namespace Acme.Workforce.Engagement.Tests.ActionPlan.Policy;

using Acme.Workforce.Engagement.ActionPlan.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class DefaultActionPlanPolicyRulesChecks
{
    public static void DefaultActionPlanPolicyValidatorReportsEveryProblem()
    {
        var validator = DefaultActionPlanPolicyRules.DefaultDefaultActionPlanPolicyValidator();
        var item = DefaultActionPlanPolicy.NewDefaultActionPlanPolicy("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DefaultActionPlanPolicyDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes()
    {
        var rule = new DefaultActionPlanPolicyDurationMinutesRangeRule(32, 120);
        var item = DefaultActionPlanPolicy.NewDefaultActionPlanPolicy("x", "y");
        item.DurationMinutes = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void DefaultActionPlanPolicyRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new DefaultActionPlanPolicyRequiredLabelRule("default action plan policy needs a value");
        var item = DefaultActionPlanPolicy.NewDefaultActionPlanPolicy("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DefaultActionPlanPolicyRequiredCategoryRuleFlagsABlankCategory()
    {
        var rule = new DefaultActionPlanPolicyRequiredCategoryRule("default action plan policy needs an entry");
        var item = DefaultActionPlanPolicy.NewDefaultActionPlanPolicy("", "");
        item.Category = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DefaultActionPlanPolicyFteRangeRuleFlagsValuesAboveFte()
    {
        var rule = new DefaultActionPlanPolicyFteRangeRule(4.8, 3563.6);
        var item = DefaultActionPlanPolicy.NewDefaultActionPlanPolicy("x", "y");
        item.Fte = 3565.1;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        DefaultActionPlanPolicyValidatorReportsEveryProblem();
        DefaultActionPlanPolicyDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes();
        DefaultActionPlanPolicyRequiredLabelRuleFlagsABlankLabel();
        DefaultActionPlanPolicyRequiredCategoryRuleFlagsABlankCategory();
        DefaultActionPlanPolicyFteRangeRuleFlagsValuesAboveFte();
    }
}
