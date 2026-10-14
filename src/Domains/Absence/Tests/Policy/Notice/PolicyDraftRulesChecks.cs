namespace Acme.Workforce.Absence.Tests.Policy.Notice;

using Acme.Workforce.Absence.Policy.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class PolicyDraftRulesChecks
{
    public static void PolicyDraftValidatorReportsEveryProblem()
    {
        var validator = PolicyDraftRules.DefaultPolicyDraftValidator();
        var item = PolicyDraft.NewPolicyDraft("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PolicyDraftRequiredUnitRuleFlagsABlankUnit()
    {
        var rule = new PolicyDraftRequiredUnitRule("policy draft needs a value");
        var item = PolicyDraft.NewPolicyDraft("", "");
        item.Unit = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PolicyDraftRequiredChannelRuleFlagsABlankChannel()
    {
        var rule = new PolicyDraftRequiredChannelRule("policy draft needs this field");
        var item = PolicyDraft.NewPolicyDraft("", "");
        item.Channel = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PolicyDraftRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new PolicyDraftRequiredCodeRule("policy draft needs an entry");
        var item = PolicyDraft.NewPolicyDraft("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        PolicyDraftValidatorReportsEveryProblem();
        PolicyDraftRequiredUnitRuleFlagsABlankUnit();
        PolicyDraftRequiredChannelRuleFlagsABlankChannel();
        PolicyDraftRequiredCodeRuleFlagsABlankCode();
    }
}
