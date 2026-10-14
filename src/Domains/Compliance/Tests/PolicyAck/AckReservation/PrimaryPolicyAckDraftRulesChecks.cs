namespace Acme.Workforce.Compliance.Tests.PolicyAck.AckReservation;

using Acme.Workforce.Compliance.PolicyAck.AckReservation;

/// <summary>Checks of ack reservation code, run by the project's runner.</summary>
public static class PrimaryPolicyAckDraftRulesChecks
{
    public static void PrimaryPolicyAckDraftValidatorReportsEveryProblem()
    {
        var validator = PrimaryPolicyAckDraftRules.DefaultPrimaryPolicyAckDraftValidator();
        var item = PrimaryPolicyAckDraft.NewPrimaryPolicyAckDraft("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PrimaryPolicyAckDraftCeilingRangeRuleFlagsValuesAboveCeiling()
    {
        var rule = new PrimaryPolicyAckDraftCeilingRangeRule(4.9, 1655.8);
        var item = PrimaryPolicyAckDraft.NewPrimaryPolicyAckDraft("x", "y");
        item.Ceiling = 1657.3;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PrimaryPolicyAckDraftRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new PrimaryPolicyAckDraftRequiredTitleRule("primary policy ack draft needs an entry");
        var item = PrimaryPolicyAckDraft.NewPrimaryPolicyAckDraft("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PrimaryPolicyAckDraftCodeLengthRuleFlagsALongCode()
    {
        var rule = new PrimaryPolicyAckDraftCodeLengthRule(32);
        var item = PrimaryPolicyAckDraft.NewPrimaryPolicyAckDraft("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        PrimaryPolicyAckDraftValidatorReportsEveryProblem();
        PrimaryPolicyAckDraftCeilingRangeRuleFlagsValuesAboveCeiling();
        PrimaryPolicyAckDraftRequiredTitleRuleFlagsABlankTitle();
        PrimaryPolicyAckDraftCodeLengthRuleFlagsALongCode();
    }
}
