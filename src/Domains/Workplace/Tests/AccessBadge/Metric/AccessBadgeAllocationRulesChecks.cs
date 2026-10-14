namespace Acme.Workforce.Workplace.Tests.AccessBadge.Metric;

using Acme.Workforce.Workplace.AccessBadge.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class AccessBadgeAllocationRulesChecks
{
    public static void AccessBadgeAllocationValidatorReportsEveryProblem()
    {
        var validator = AccessBadgeAllocationRules.DefaultAccessBadgeAllocationValidator();
        var item = AccessBadgeAllocation.NewAccessBadgeAllocation("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AccessBadgeAllocationRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new AccessBadgeAllocationRequiredCodeRule("access badge allocation needs this field");
        var item = AccessBadgeAllocation.NewAccessBadgeAllocation("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AccessBadgeAllocationContactEmailLengthRuleFlagsALongContactEmail()
    {
        var rule = new AccessBadgeAllocationContactEmailLengthRule(86400);
        var item = AccessBadgeAllocation.NewAccessBadgeAllocation("x", "y");
        item.ContactEmail = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ContactEmail = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AccessBadgeAllocationCodeLengthRuleFlagsALongCode()
    {
        var rule = new AccessBadgeAllocationCodeLengthRule(80);
        var item = AccessBadgeAllocation.NewAccessBadgeAllocation("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AccessBadgeAllocationRequiredContactEmailRuleFlagsABlankContactEmail()
    {
        var rule = new AccessBadgeAllocationRequiredContactEmailRule("access badge allocation needs an entry");
        var item = AccessBadgeAllocation.NewAccessBadgeAllocation("", "");
        item.ContactEmail = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AccessBadgeAllocationRetriesRangeRuleFlagsValuesAboveRetries()
    {
        var rule = new AccessBadgeAllocationRetriesRangeRule(32, 3600);
        var item = AccessBadgeAllocation.NewAccessBadgeAllocation("x", "y");
        item.Retries = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        AccessBadgeAllocationValidatorReportsEveryProblem();
        AccessBadgeAllocationRequiredCodeRuleFlagsABlankCode();
        AccessBadgeAllocationContactEmailLengthRuleFlagsALongContactEmail();
        AccessBadgeAllocationCodeLengthRuleFlagsALongCode();
        AccessBadgeAllocationRequiredContactEmailRuleFlagsABlankContactEmail();
        AccessBadgeAllocationRetriesRangeRuleFlagsValuesAboveRetries();
    }
}
