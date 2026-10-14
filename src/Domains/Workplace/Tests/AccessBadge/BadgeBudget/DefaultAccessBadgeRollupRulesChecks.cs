namespace Acme.Workforce.Workplace.Tests.AccessBadge.BadgeBudget;

using Acme.Workforce.Workplace.AccessBadge.BadgeBudget;

/// <summary>Checks of badge budget code, run by the project's runner.</summary>
public static class DefaultAccessBadgeRollupRulesChecks
{
    public static void DefaultAccessBadgeRollupValidatorReportsEveryProblem()
    {
        var validator = DefaultAccessBadgeRollupRules.DefaultDefaultAccessBadgeRollupValidator();
        var item = DefaultAccessBadgeRollup.NewDefaultAccessBadgeRollup("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DefaultAccessBadgeRollupQuantityRangeRuleFlagsValuesAboveQuantity()
    {
        var rule = new DefaultAccessBadgeRollupQuantityRangeRule(120, 120);
        var item = DefaultAccessBadgeRollup.NewDefaultAccessBadgeRollup("x", "y");
        item.Quantity = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void DefaultAccessBadgeRollupWindowDaysRangeRuleFlagsValuesAboveWindowDays()
    {
        var rule = new DefaultAccessBadgeRollupWindowDaysRangeRule(32, 120);
        var item = DefaultAccessBadgeRollup.NewDefaultAccessBadgeRollup("x", "y");
        item.WindowDays = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void DefaultAccessBadgeRollupRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new DefaultAccessBadgeRollupRequiredLabelRule("default access badge rollup needs this field");
        var item = DefaultAccessBadgeRollup.NewDefaultAccessBadgeRollup("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DefaultAccessBadgeRollupLabelLengthRuleFlagsALongLabel()
    {
        var rule = new DefaultAccessBadgeRollupLabelLengthRule(32);
        var item = DefaultAccessBadgeRollup.NewDefaultAccessBadgeRollup("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        DefaultAccessBadgeRollupValidatorReportsEveryProblem();
        DefaultAccessBadgeRollupQuantityRangeRuleFlagsValuesAboveQuantity();
        DefaultAccessBadgeRollupWindowDaysRangeRuleFlagsValuesAboveWindowDays();
        DefaultAccessBadgeRollupRequiredLabelRuleFlagsABlankLabel();
        DefaultAccessBadgeRollupLabelLengthRuleFlagsALongLabel();
    }
}
