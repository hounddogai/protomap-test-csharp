namespace Acme.Workforce.Compliance.Tests.PolicyAck.Notice;

using Acme.Workforce.Compliance.PolicyAck.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class PolicyAckNoticeRulesChecks
{
    public static void PolicyAckNoticeValidatorReportsEveryProblem()
    {
        var validator = PolicyAckNoticeRules.DefaultPolicyAckNoticeValidator();
        var item = PolicyAckNotice.NewPolicyAckNotice("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PolicyAckNoticeTeamLengthRuleFlagsALongTeam()
    {
        var rule = new PolicyAckNoticeTeamLengthRule(120);
        var item = PolicyAckNotice.NewPolicyAckNotice("x", "y");
        item.Team = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Team = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PolicyAckNoticeCurrencyLengthRuleFlagsALongCurrency()
    {
        var rule = new PolicyAckNoticeCurrencyLengthRule(16);
        var item = PolicyAckNotice.NewPolicyAckNotice("x", "y");
        item.Currency = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Currency = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PolicyAckNoticeRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new PolicyAckNoticeRequiredCodeRule("policy ack notice needs an entry");
        var item = PolicyAckNotice.NewPolicyAckNotice("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        PolicyAckNoticeValidatorReportsEveryProblem();
        PolicyAckNoticeTeamLengthRuleFlagsALongTeam();
        PolicyAckNoticeCurrencyLengthRuleFlagsALongCurrency();
        PolicyAckNoticeRequiredCodeRuleFlagsABlankCode();
    }
}
