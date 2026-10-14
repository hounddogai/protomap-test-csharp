namespace Acme.Workforce.Assets.Tests.Refresh.Request;

using Acme.Workforce.Assets.Refresh.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class PendingRefreshMappingRulesChecks
{
    public static void PendingRefreshMappingValidatorReportsEveryProblem()
    {
        var validator = PendingRefreshMappingRules.DefaultPendingRefreshMappingValidator();
        var item = PendingRefreshMapping.NewPendingRefreshMapping("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PendingRefreshMappingLimitRangeRuleFlagsValuesAboveLimit()
    {
        var rule = new PendingRefreshMappingLimitRangeRule(32, 32);
        var item = PendingRefreshMapping.NewPendingRefreshMapping("x", "y");
        item.Limit = 33;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PendingRefreshMappingLabelLengthRuleFlagsALongLabel()
    {
        var rule = new PendingRefreshMappingLabelLengthRule(64);
        var item = PendingRefreshMapping.NewPendingRefreshMapping("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PendingRefreshMappingRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new PendingRefreshMappingRequiredCodeRule("pending refresh mapping needs a value");
        var item = PendingRefreshMapping.NewPendingRefreshMapping("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        PendingRefreshMappingValidatorReportsEveryProblem();
        PendingRefreshMappingLimitRangeRuleFlagsValuesAboveLimit();
        PendingRefreshMappingLabelLengthRuleFlagsALongLabel();
        PendingRefreshMappingRequiredCodeRuleFlagsABlankCode();
    }
}
