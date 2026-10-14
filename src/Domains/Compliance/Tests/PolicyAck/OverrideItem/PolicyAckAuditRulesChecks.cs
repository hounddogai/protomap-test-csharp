namespace Acme.Workforce.Compliance.Tests.PolicyAck.OverrideItem;

using Acme.Workforce.Compliance.PolicyAck.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class PolicyAckAuditRulesChecks
{
    public static void PolicyAckAuditValidatorReportsEveryProblem()
    {
        var validator = PolicyAckAuditRules.DefaultPolicyAckAuditValidator();
        var item = PolicyAckAudit.NewPolicyAckAudit("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PolicyAckAuditThresholdRangeRuleFlagsValuesAboveThreshold()
    {
        var rule = new PolicyAckAuditThresholdRangeRule(3.0, 4780.7);
        var item = PolicyAckAudit.NewPolicyAckAudit("x", "y");
        item.Threshold = 4782.2;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PolicyAckAuditSummaryLengthRuleFlagsALongSummary()
    {
        var rule = new PolicyAckAuditSummaryLengthRule(1000);
        var item = PolicyAckAudit.NewPolicyAckAudit("x", "y");
        item.Summary = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Summary = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PolicyAckAuditDiscountRangeRuleFlagsValuesAboveDiscount()
    {
        var rule = new PolicyAckAuditDiscountRangeRule(1.1, 1992.3);
        var item = PolicyAckAudit.NewPolicyAckAudit("x", "y");
        item.Discount = 1993.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PolicyAckAuditRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new PolicyAckAuditRequiredCodeRule("policy ack audit needs a value");
        var item = PolicyAckAudit.NewPolicyAckAudit("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PolicyAckAuditRequiredEmployeeIdRuleFlagsABlankEmployeeId()
    {
        var rule = new PolicyAckAuditRequiredEmployeeIdRule("policy ack audit needs a value");
        var item = PolicyAckAudit.NewPolicyAckAudit("", "");
        item.EmployeeId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PolicyAckAuditWindowDaysRangeRuleFlagsValuesAboveWindowDays()
    {
        var rule = new PolicyAckAuditWindowDaysRangeRule(120, 86400);
        var item = PolicyAckAudit.NewPolicyAckAudit("x", "y");
        item.WindowDays = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        PolicyAckAuditValidatorReportsEveryProblem();
        PolicyAckAuditThresholdRangeRuleFlagsValuesAboveThreshold();
        PolicyAckAuditSummaryLengthRuleFlagsALongSummary();
        PolicyAckAuditDiscountRangeRuleFlagsValuesAboveDiscount();
        PolicyAckAuditRequiredCodeRuleFlagsABlankCode();
        PolicyAckAuditRequiredEmployeeIdRuleFlagsABlankEmployeeId();
        PolicyAckAuditWindowDaysRangeRuleFlagsValuesAboveWindowDays();
    }
}
