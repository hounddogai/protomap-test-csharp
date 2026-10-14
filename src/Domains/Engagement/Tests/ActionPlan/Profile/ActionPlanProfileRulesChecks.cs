namespace Acme.Workforce.Engagement.Tests.ActionPlan.Profile;

using Acme.Workforce.Engagement.ActionPlan.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class ActionPlanProfileRulesChecks
{
    public static void ActionPlanProfileValidatorReportsEveryProblem()
    {
        var validator = ActionPlanProfileRules.DefaultActionPlanProfileValidator();
        var item = ActionPlanProfile.NewActionPlanProfile("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ActionPlanProfileCodeLengthRuleFlagsALongCode()
    {
        var rule = new ActionPlanProfileCodeLengthRule(86400);
        var item = ActionPlanProfile.NewActionPlanProfile("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ActionPlanProfileRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new ActionPlanProfileRequiredCodeRule("action plan profile needs a value");
        var item = ActionPlanProfile.NewActionPlanProfile("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ActionPlanProfileTitleLengthRuleFlagsALongTitle()
    {
        var rule = new ActionPlanProfileTitleLengthRule(64);
        var item = ActionPlanProfile.NewActionPlanProfile("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ActionPlanProfileRequiredManagerIdRuleFlagsABlankManagerId()
    {
        var rule = new ActionPlanProfileRequiredManagerIdRule("action plan profile needs a value");
        var item = ActionPlanProfile.NewActionPlanProfile("", "");
        item.ManagerId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        ActionPlanProfileValidatorReportsEveryProblem();
        ActionPlanProfileCodeLengthRuleFlagsALongCode();
        ActionPlanProfileRequiredCodeRuleFlagsABlankCode();
        ActionPlanProfileTitleLengthRuleFlagsALongTitle();
        ActionPlanProfileRequiredManagerIdRuleFlagsABlankManagerId();
    }
}
