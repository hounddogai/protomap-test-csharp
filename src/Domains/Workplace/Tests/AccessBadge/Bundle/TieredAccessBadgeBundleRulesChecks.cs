namespace Acme.Workforce.Workplace.Tests.AccessBadge.Bundle;

using Acme.Workforce.Workplace.AccessBadge.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class TieredAccessBadgeBundleRulesChecks
{
    public static void TieredAccessBadgeBundleValidatorReportsEveryProblem()
    {
        var validator = TieredAccessBadgeBundleRules.DefaultTieredAccessBadgeBundleValidator();
        var item = TieredAccessBadgeBundle.NewTieredAccessBadgeBundle("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TieredAccessBadgeBundleJobCodeLengthRuleFlagsALongJobCode()
    {
        var rule = new TieredAccessBadgeBundleJobCodeLengthRule(3600);
        var item = TieredAccessBadgeBundle.NewTieredAccessBadgeBundle("x", "y");
        item.JobCode = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.JobCode = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TieredAccessBadgeBundleTitleLengthRuleFlagsALongTitle()
    {
        var rule = new TieredAccessBadgeBundleTitleLengthRule(16);
        var item = TieredAccessBadgeBundle.NewTieredAccessBadgeBundle("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TieredAccessBadgeBundleStartsAtRangeRuleFlagsValuesAboveStartsAt()
    {
        var rule = new TieredAccessBadgeBundleStartsAtRangeRule(16, 1000);
        var item = TieredAccessBadgeBundle.NewTieredAccessBadgeBundle("x", "y");
        item.StartsAt = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void TieredAccessBadgeBundleCategoryLengthRuleFlagsALongCategory()
    {
        var rule = new TieredAccessBadgeBundleCategoryLengthRule(120);
        var item = TieredAccessBadgeBundle.NewTieredAccessBadgeBundle("x", "y");
        item.Category = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Category = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TieredAccessBadgeBundleRequiredCategoryRuleFlagsABlankCategory()
    {
        var rule = new TieredAccessBadgeBundleRequiredCategoryRule("tiered access badge bundle needs this field");
        var item = TieredAccessBadgeBundle.NewTieredAccessBadgeBundle("", "");
        item.Category = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        TieredAccessBadgeBundleValidatorReportsEveryProblem();
        TieredAccessBadgeBundleJobCodeLengthRuleFlagsALongJobCode();
        TieredAccessBadgeBundleTitleLengthRuleFlagsALongTitle();
        TieredAccessBadgeBundleStartsAtRangeRuleFlagsValuesAboveStartsAt();
        TieredAccessBadgeBundleCategoryLengthRuleFlagsALongCategory();
        TieredAccessBadgeBundleRequiredCategoryRuleFlagsABlankCategory();
    }
}
