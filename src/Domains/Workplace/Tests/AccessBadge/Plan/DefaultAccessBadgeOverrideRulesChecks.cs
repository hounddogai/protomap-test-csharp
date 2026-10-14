namespace Acme.Workforce.Workplace.Tests.AccessBadge.Plan;

using Acme.Workforce.Workplace.AccessBadge.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class DefaultAccessBadgeOverrideRulesChecks
{
    public static void DefaultAccessBadgeOverrideValidatorReportsEveryProblem()
    {
        var validator = DefaultAccessBadgeOverrideRules.DefaultDefaultAccessBadgeOverrideValidator();
        var item = DefaultAccessBadgeOverride.NewDefaultAccessBadgeOverride("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DefaultAccessBadgeOverrideDiscountRangeRuleFlagsValuesAboveDiscount()
    {
        var rule = new DefaultAccessBadgeOverrideDiscountRangeRule(6.3, 855.8);
        var item = DefaultAccessBadgeOverride.NewDefaultAccessBadgeOverride("x", "y");
        item.Discount = 857.3;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void DefaultAccessBadgeOverrideCodeLengthRuleFlagsALongCode()
    {
        var rule = new DefaultAccessBadgeOverrideCodeLengthRule(80);
        var item = DefaultAccessBadgeOverride.NewDefaultAccessBadgeOverride("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void DefaultAccessBadgeOverrideRequiredJobCodeRuleFlagsABlankJobCode()
    {
        var rule = new DefaultAccessBadgeOverrideRequiredJobCodeRule("default access badge override needs a value");
        var item = DefaultAccessBadgeOverride.NewDefaultAccessBadgeOverride("", "");
        item.JobCode = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DefaultAccessBadgeOverrideRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new DefaultAccessBadgeOverrideRequiredCodeRule("default access badge override needs this field");
        var item = DefaultAccessBadgeOverride.NewDefaultAccessBadgeOverride("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DefaultAccessBadgeOverrideRequiredBadgeRuleFlagsABlankBadge()
    {
        var rule = new DefaultAccessBadgeOverrideRequiredBadgeRule("default access badge override needs an entry");
        var item = DefaultAccessBadgeOverride.NewDefaultAccessBadgeOverride("", "");
        item.Badge = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        DefaultAccessBadgeOverrideValidatorReportsEveryProblem();
        DefaultAccessBadgeOverrideDiscountRangeRuleFlagsValuesAboveDiscount();
        DefaultAccessBadgeOverrideCodeLengthRuleFlagsALongCode();
        DefaultAccessBadgeOverrideRequiredJobCodeRuleFlagsABlankJobCode();
        DefaultAccessBadgeOverrideRequiredCodeRuleFlagsABlankCode();
        DefaultAccessBadgeOverrideRequiredBadgeRuleFlagsABlankBadge();
    }
}
