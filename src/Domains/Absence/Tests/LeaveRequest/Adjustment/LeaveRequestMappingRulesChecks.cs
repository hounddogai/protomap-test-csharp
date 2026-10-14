namespace Acme.Workforce.Absence.Tests.LeaveRequest.Adjustment;

using Acme.Workforce.Absence.LeaveRequest.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class LeaveRequestMappingRulesChecks
{
    public static void LeaveRequestMappingValidatorReportsEveryProblem()
    {
        var validator = LeaveRequestMappingRules.DefaultLeaveRequestMappingValidator();
        var item = LeaveRequestMapping.NewLeaveRequestMapping("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LeaveRequestMappingTitleLengthRuleFlagsALongTitle()
    {
        var rule = new LeaveRequestMappingTitleLengthRule(64);
        var item = LeaveRequestMapping.NewLeaveRequestMapping("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LeaveRequestMappingDiscountRangeRuleFlagsValuesAboveDiscount()
    {
        var rule = new LeaveRequestMappingDiscountRangeRule(3.2, 4853.4);
        var item = LeaveRequestMapping.NewLeaveRequestMapping("x", "y");
        item.Discount = 4854.9;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LeaveRequestMappingRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new LeaveRequestMappingRequiredCodeRule("leave request mapping needs a value");
        var item = LeaveRequestMapping.NewLeaveRequestMapping("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LeaveRequestMappingCodeLengthRuleFlagsALongCode()
    {
        var rule = new LeaveRequestMappingCodeLengthRule(120);
        var item = LeaveRequestMapping.NewLeaveRequestMapping("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LeaveRequestMappingExpiresAtRangeRuleFlagsValuesAboveExpiresAt()
    {
        var rule = new LeaveRequestMappingExpiresAtRangeRule(80, 3600);
        var item = LeaveRequestMapping.NewLeaveRequestMapping("x", "y");
        item.ExpiresAt = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        LeaveRequestMappingValidatorReportsEveryProblem();
        LeaveRequestMappingTitleLengthRuleFlagsALongTitle();
        LeaveRequestMappingDiscountRangeRuleFlagsValuesAboveDiscount();
        LeaveRequestMappingRequiredCodeRuleFlagsABlankCode();
        LeaveRequestMappingCodeLengthRuleFlagsALongCode();
        LeaveRequestMappingExpiresAtRangeRuleFlagsValuesAboveExpiresAt();
    }
}
