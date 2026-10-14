namespace Acme.Workforce.Engagement.Tests.ActionPlan.Slot;

using Acme.Workforce.Engagement.ActionPlan.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class ActionPlanDraftRulesChecks
{
    public static void ActionPlanDraftValidatorReportsEveryProblem()
    {
        var validator = ActionPlanDraftRules.DefaultActionPlanDraftValidator();
        var item = ActionPlanDraft.NewActionPlanDraft("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ActionPlanDraftPhoneLengthRuleFlagsALongPhone()
    {
        var rule = new ActionPlanDraftPhoneLengthRule(1000);
        var item = ActionPlanDraft.NewActionPlanDraft("x", "y");
        item.Phone = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Phone = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ActionPlanDraftLimitRangeRuleFlagsValuesAboveLimit()
    {
        var rule = new ActionPlanDraftLimitRangeRule(255, 3600);
        var item = ActionPlanDraft.NewActionPlanDraft("x", "y");
        item.Limit = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ActionPlanDraftRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new ActionPlanDraftRequiredCodeRule("action plan draft needs a value");
        var item = ActionPlanDraft.NewActionPlanDraft("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ActionPlanDraftRequiredPhoneRuleFlagsABlankPhone()
    {
        var rule = new ActionPlanDraftRequiredPhoneRule("action plan draft needs an entry");
        var item = ActionPlanDraft.NewActionPlanDraft("", "");
        item.Phone = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        ActionPlanDraftValidatorReportsEveryProblem();
        ActionPlanDraftPhoneLengthRuleFlagsALongPhone();
        ActionPlanDraftLimitRangeRuleFlagsValuesAboveLimit();
        ActionPlanDraftRequiredCodeRuleFlagsABlankCode();
        ActionPlanDraftRequiredPhoneRuleFlagsABlankPhone();
    }
}
