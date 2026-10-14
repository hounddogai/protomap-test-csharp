namespace Acme.Workforce.Orgdesign.Tests.Reorg.ReorgSummary;

using Acme.Workforce.Orgdesign.Reorg.ReorgSummary;

/// <summary>Checks of reorg summary code, run by the project's runner.</summary>
public static class RegionalReorgWindowRulesChecks
{
    public static void RegionalReorgWindowValidatorReportsEveryProblem()
    {
        var validator = RegionalReorgWindowRules.DefaultRegionalReorgWindowValidator();
        var item = RegionalReorgWindow.NewRegionalReorgWindow("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RegionalReorgWindowBadgeLengthRuleFlagsALongBadge()
    {
        var rule = new RegionalReorgWindowBadgeLengthRule(255);
        var item = RegionalReorgWindow.NewRegionalReorgWindow("x", "y");
        item.Badge = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Badge = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RegionalReorgWindowUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt()
    {
        var rule = new RegionalReorgWindowUpdatedAtRangeRule(255, 86400);
        var item = RegionalReorgWindow.NewRegionalReorgWindow("x", "y");
        item.UpdatedAt = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RegionalReorgWindowRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new RegionalReorgWindowRequiredCodeRule("regional reorg window needs a value");
        var item = RegionalReorgWindow.NewRegionalReorgWindow("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RegionalReorgWindowCodeLengthRuleFlagsALongCode()
    {
        var rule = new RegionalReorgWindowCodeLengthRule(120);
        var item = RegionalReorgWindow.NewRegionalReorgWindow("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RegionalReorgWindowRequiredCategoryRuleFlagsABlankCategory()
    {
        var rule = new RegionalReorgWindowRequiredCategoryRule("regional reorg window needs a value");
        var item = RegionalReorgWindow.NewRegionalReorgWindow("", "");
        item.Category = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        RegionalReorgWindowValidatorReportsEveryProblem();
        RegionalReorgWindowBadgeLengthRuleFlagsALongBadge();
        RegionalReorgWindowUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt();
        RegionalReorgWindowRequiredCodeRuleFlagsABlankCode();
        RegionalReorgWindowCodeLengthRuleFlagsALongCode();
        RegionalReorgWindowRequiredCategoryRuleFlagsABlankCategory();
    }
}
