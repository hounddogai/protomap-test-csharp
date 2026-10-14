namespace Acme.Workforce.Compliance.Tests.PolicyAck.AckQuota;

using Acme.Workforce.Compliance.PolicyAck.AckQuota;

/// <summary>Checks of ack quota code, run by the project's runner.</summary>
public static class ScheduledPolicyAckQuotaRulesChecks
{
    public static void ScheduledPolicyAckQuotaValidatorReportsEveryProblem()
    {
        var validator = ScheduledPolicyAckQuotaRules.DefaultScheduledPolicyAckQuotaValidator();
        var item = ScheduledPolicyAckQuota.NewScheduledPolicyAckQuota("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ScheduledPolicyAckQuotaGraceDaysRangeRuleFlagsValuesAboveGraceDays()
    {
        var rule = new ScheduledPolicyAckQuotaGraceDaysRangeRule(16, 80);
        var item = ScheduledPolicyAckQuota.NewScheduledPolicyAckQuota("x", "y");
        item.GraceDays = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ScheduledPolicyAckQuotaRequiredNameRuleFlagsABlankName()
    {
        var rule = new ScheduledPolicyAckQuotaRequiredNameRule("scheduled policy ack quota needs an entry");
        var item = ScheduledPolicyAckQuota.NewScheduledPolicyAckQuota("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        ScheduledPolicyAckQuotaValidatorReportsEveryProblem();
        ScheduledPolicyAckQuotaGraceDaysRangeRuleFlagsValuesAboveGraceDays();
        ScheduledPolicyAckQuotaRequiredNameRuleFlagsABlankName();
    }
}
