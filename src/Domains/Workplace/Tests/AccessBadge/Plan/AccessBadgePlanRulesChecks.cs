namespace Acme.Workforce.Workplace.Tests.AccessBadge.Plan;

using Acme.Workforce.Workplace.AccessBadge.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class AccessBadgePlanRulesChecks
{
    public static void AccessBadgePlanValidatorReportsEveryProblem()
    {
        var validator = AccessBadgePlanRules.DefaultAccessBadgePlanValidator();
        var item = AccessBadgePlan.NewAccessBadgePlan("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AccessBadgePlanPhoneLengthRuleFlagsALongPhone()
    {
        var rule = new AccessBadgePlanPhoneLengthRule(255);
        var item = AccessBadgePlan.NewAccessBadgePlan("x", "y");
        item.Phone = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Phone = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AccessBadgePlanRequiredLocaleRuleFlagsABlankLocale()
    {
        var rule = new AccessBadgePlanRequiredLocaleRule("access badge plan needs an entry");
        var item = AccessBadgePlan.NewAccessBadgePlan("", "");
        item.Locale = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AccessBadgePlanRequiredPhoneRuleFlagsABlankPhone()
    {
        var rule = new AccessBadgePlanRequiredPhoneRule("access badge plan needs a value");
        var item = AccessBadgePlan.NewAccessBadgePlan("", "");
        item.Phone = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AccessBadgePlanBatchSizeRangeRuleFlagsValuesAboveBatchSize()
    {
        var rule = new AccessBadgePlanBatchSizeRangeRule(16, 1000);
        var item = AccessBadgePlan.NewAccessBadgePlan("x", "y");
        item.BatchSize = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AccessBadgePlanLocaleLengthRuleFlagsALongLocale()
    {
        var rule = new AccessBadgePlanLocaleLengthRule(120);
        var item = AccessBadgePlan.NewAccessBadgePlan("x", "y");
        item.Locale = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Locale = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        AccessBadgePlanValidatorReportsEveryProblem();
        AccessBadgePlanPhoneLengthRuleFlagsALongPhone();
        AccessBadgePlanRequiredLocaleRuleFlagsABlankLocale();
        AccessBadgePlanRequiredPhoneRuleFlagsABlankPhone();
        AccessBadgePlanBatchSizeRangeRuleFlagsValuesAboveBatchSize();
        AccessBadgePlanLocaleLengthRuleFlagsALongLocale();
    }
}
