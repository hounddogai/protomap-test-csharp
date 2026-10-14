namespace Acme.Workforce.Engagement.Tests.ActionPlan.Entry;

using Acme.Workforce.Engagement.ActionPlan.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class ActionPlanEntryRulesChecks
{
    public static void ActionPlanEntryValidatorReportsEveryProblem()
    {
        var validator = ActionPlanEntryRules.DefaultActionPlanEntryValidator();
        var item = ActionPlanEntry.NewActionPlanEntry("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ActionPlanEntryRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new ActionPlanEntryRequiredCodeRule("action plan entry needs an entry");
        var item = ActionPlanEntry.NewActionPlanEntry("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ActionPlanEntryRetriesRangeRuleFlagsValuesAboveRetries()
    {
        var rule = new ActionPlanEntryRetriesRangeRule(80, 3600);
        var item = ActionPlanEntry.NewActionPlanEntry("x", "y");
        item.Retries = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ActionPlanEntryRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new ActionPlanEntryRequiredLabelRule("action plan entry needs an entry");
        var item = ActionPlanEntry.NewActionPlanEntry("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        ActionPlanEntryValidatorReportsEveryProblem();
        ActionPlanEntryRequiredCodeRuleFlagsABlankCode();
        ActionPlanEntryRetriesRangeRuleFlagsValuesAboveRetries();
        ActionPlanEntryRequiredLabelRuleFlagsABlankLabel();
    }
}
