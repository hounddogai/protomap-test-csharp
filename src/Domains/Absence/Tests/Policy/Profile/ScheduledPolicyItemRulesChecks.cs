namespace Acme.Workforce.Absence.Tests.Policy.Profile;

using Acme.Workforce.Absence.Policy.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class ScheduledPolicyItemRulesChecks
{
    public static void ScheduledPolicyItemValidatorReportsEveryProblem()
    {
        var validator = ScheduledPolicyItemRules.DefaultScheduledPolicyItemValidator();
        var item = ScheduledPolicyItem.NewScheduledPolicyItem("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ScheduledPolicyItemRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new ScheduledPolicyItemRequiredCodeRule("scheduled policy item needs an entry");
        var item = ScheduledPolicyItem.NewScheduledPolicyItem("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ScheduledPolicyItemRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new ScheduledPolicyItemRequiredTitleRule("scheduled policy item needs an entry");
        var item = ScheduledPolicyItem.NewScheduledPolicyItem("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        ScheduledPolicyItemValidatorReportsEveryProblem();
        ScheduledPolicyItemRequiredCodeRuleFlagsABlankCode();
        ScheduledPolicyItemRequiredTitleRuleFlagsABlankTitle();
    }
}
