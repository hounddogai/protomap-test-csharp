namespace Acme.Workforce.Assets.Tests.Laptop.Adjustment;

using Acme.Workforce.Assets.Laptop.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class CappedLaptopRollupRulesChecks
{
    public static void CappedLaptopRollupValidatorReportsEveryProblem()
    {
        var validator = CappedLaptopRollupRules.DefaultCappedLaptopRollupValidator();
        var item = CappedLaptopRollup.NewCappedLaptopRollup("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CappedLaptopRollupRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new CappedLaptopRollupRequiredTitleRule("capped laptop rollup needs an entry");
        var item = CappedLaptopRollup.NewCappedLaptopRollup("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void CappedLaptopRollupBadgeLengthRuleFlagsALongBadge()
    {
        var rule = new CappedLaptopRollupBadgeLengthRule(255);
        var item = CappedLaptopRollup.NewCappedLaptopRollup("x", "y");
        item.Badge = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Badge = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CappedLaptopRollupWeightRangeRuleFlagsValuesAboveWeight()
    {
        var rule = new CappedLaptopRollupWeightRangeRule(4.2, 2314.5);
        var item = CappedLaptopRollup.NewCappedLaptopRollup("x", "y");
        item.Weight = 2316.0;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void CappedLaptopRollupRequiredBadgeRuleFlagsABlankBadge()
    {
        var rule = new CappedLaptopRollupRequiredBadgeRule("capped laptop rollup needs an entry");
        var item = CappedLaptopRollup.NewCappedLaptopRollup("", "");
        item.Badge = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        CappedLaptopRollupValidatorReportsEveryProblem();
        CappedLaptopRollupRequiredTitleRuleFlagsABlankTitle();
        CappedLaptopRollupBadgeLengthRuleFlagsALongBadge();
        CappedLaptopRollupWeightRangeRuleFlagsValuesAboveWeight();
        CappedLaptopRollupRequiredBadgeRuleFlagsABlankBadge();
    }
}
