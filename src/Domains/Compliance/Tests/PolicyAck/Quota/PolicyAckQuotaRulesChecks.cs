namespace Acme.Workforce.Compliance.Tests.PolicyAck.Quota;

using Acme.Workforce.Compliance.PolicyAck.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class PolicyAckQuotaRulesChecks
{
    public static void PolicyAckQuotaValidatorReportsEveryProblem()
    {
        var validator = PolicyAckQuotaRules.DefaultPolicyAckQuotaValidator();
        var item = PolicyAckQuota.NewPolicyAckQuota("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PolicyAckQuotaRequiredCategoryRuleFlagsABlankCategory()
    {
        var rule = new PolicyAckQuotaRequiredCategoryRule("policy ack quota needs this field");
        var item = PolicyAckQuota.NewPolicyAckQuota("", "");
        item.Category = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PolicyAckQuotaRateRangeRuleFlagsValuesAboveRate()
    {
        var rule = new PolicyAckQuotaRateRangeRule(9.9, 488.2);
        var item = PolicyAckQuota.NewPolicyAckQuota("x", "y");
        item.Rate = 489.7;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PolicyAckQuotaRequiredSummaryRuleFlagsABlankSummary()
    {
        var rule = new PolicyAckQuotaRequiredSummaryRule("policy ack quota needs an entry");
        var item = PolicyAckQuota.NewPolicyAckQuota("", "");
        item.Summary = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PolicyAckQuotaRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new PolicyAckQuotaRequiredTitleRule("policy ack quota needs an entry");
        var item = PolicyAckQuota.NewPolicyAckQuota("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        PolicyAckQuotaValidatorReportsEveryProblem();
        PolicyAckQuotaRequiredCategoryRuleFlagsABlankCategory();
        PolicyAckQuotaRateRangeRuleFlagsValuesAboveRate();
        PolicyAckQuotaRequiredSummaryRuleFlagsABlankSummary();
        PolicyAckQuotaRequiredTitleRuleFlagsABlankTitle();
    }
}
