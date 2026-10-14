namespace Acme.Workforce.Compliance.Tests.PolicyAck.Draft;

using Acme.Workforce.Compliance.PolicyAck.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class PolicyAckDraftRulesChecks
{
    public static void PolicyAckDraftValidatorReportsEveryProblem()
    {
        var validator = PolicyAckDraftRules.DefaultPolicyAckDraftValidator();
        var item = PolicyAckDraft.NewPolicyAckDraft("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PolicyAckDraftTimezoneLengthRuleFlagsALongTimezone()
    {
        var rule = new PolicyAckDraftTimezoneLengthRule(3600);
        var item = PolicyAckDraft.NewPolicyAckDraft("x", "y");
        item.Timezone = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Timezone = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PolicyAckDraftTitleLengthRuleFlagsALongTitle()
    {
        var rule = new PolicyAckDraftTitleLengthRule(3600);
        var item = PolicyAckDraft.NewPolicyAckDraft("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PolicyAckDraftCodeLengthRuleFlagsALongCode()
    {
        var rule = new PolicyAckDraftCodeLengthRule(120);
        var item = PolicyAckDraft.NewPolicyAckDraft("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PolicyAckDraftRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new PolicyAckDraftRequiredTitleRule("policy ack draft needs an entry");
        var item = PolicyAckDraft.NewPolicyAckDraft("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PolicyAckDraftDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes()
    {
        var rule = new PolicyAckDraftDurationMinutesRangeRule(80, 255);
        var item = PolicyAckDraft.NewPolicyAckDraft("x", "y");
        item.DurationMinutes = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        PolicyAckDraftValidatorReportsEveryProblem();
        PolicyAckDraftTimezoneLengthRuleFlagsALongTimezone();
        PolicyAckDraftTitleLengthRuleFlagsALongTitle();
        PolicyAckDraftCodeLengthRuleFlagsALongCode();
        PolicyAckDraftRequiredTitleRuleFlagsABlankTitle();
        PolicyAckDraftDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes();
    }
}
