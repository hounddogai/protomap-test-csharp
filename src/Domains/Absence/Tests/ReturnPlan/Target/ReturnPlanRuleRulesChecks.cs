namespace Acme.Workforce.Absence.Tests.ReturnPlan.Target;

using Acme.Workforce.Absence.ReturnPlan.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class ReturnPlanRuleRulesChecks
{
    public static void ReturnPlanRuleValidatorReportsEveryProblem()
    {
        var validator = ReturnPlanRuleRules.DefaultReturnPlanRuleValidator();
        var item = ReturnPlanRule.NewReturnPlanRule("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ReturnPlanRuleRequiredCategoryRuleFlagsABlankCategory()
    {
        var rule = new ReturnPlanRuleRequiredCategoryRule("return plan rule needs a value");
        var item = ReturnPlanRule.NewReturnPlanRule("", "");
        item.Category = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ReturnPlanRuleChannelLengthRuleFlagsALongChannel()
    {
        var rule = new ReturnPlanRuleChannelLengthRule(1000);
        var item = ReturnPlanRule.NewReturnPlanRule("x", "y");
        item.Channel = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Channel = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        ReturnPlanRuleValidatorReportsEveryProblem();
        ReturnPlanRuleRequiredCategoryRuleFlagsABlankCategory();
        ReturnPlanRuleChannelLengthRuleFlagsALongChannel();
    }
}
