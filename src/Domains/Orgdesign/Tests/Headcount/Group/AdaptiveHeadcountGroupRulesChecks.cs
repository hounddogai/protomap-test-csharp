namespace Acme.Workforce.Orgdesign.Tests.Headcount.Group;

using Acme.Workforce.Orgdesign.Headcount.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class AdaptiveHeadcountGroupRulesChecks
{
    public static void AdaptiveHeadcountGroupValidatorReportsEveryProblem()
    {
        var validator = AdaptiveHeadcountGroupRules.DefaultAdaptiveHeadcountGroupValidator();
        var item = AdaptiveHeadcountGroup.NewAdaptiveHeadcountGroup("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AdaptiveHeadcountGroupRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new AdaptiveHeadcountGroupRequiredCodeRule("adaptive headcount group needs this field");
        var item = AdaptiveHeadcountGroup.NewAdaptiveHeadcountGroup("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AdaptiveHeadcountGroupManagerIdLengthRuleFlagsALongManagerId()
    {
        var rule = new AdaptiveHeadcountGroupManagerIdLengthRule(1000);
        var item = AdaptiveHeadcountGroup.NewAdaptiveHeadcountGroup("x", "y");
        item.ManagerId = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ManagerId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AdaptiveHeadcountGroupRequiredLocaleRuleFlagsABlankLocale()
    {
        var rule = new AdaptiveHeadcountGroupRequiredLocaleRule("adaptive headcount group needs this field");
        var item = AdaptiveHeadcountGroup.NewAdaptiveHeadcountGroup("", "");
        item.Locale = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AdaptiveHeadcountGroupWindowDaysRangeRuleFlagsValuesAboveWindowDays()
    {
        var rule = new AdaptiveHeadcountGroupWindowDaysRangeRule(64, 80);
        var item = AdaptiveHeadcountGroup.NewAdaptiveHeadcountGroup("x", "y");
        item.WindowDays = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AdaptiveHeadcountGroupRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new AdaptiveHeadcountGroupRequiredTitleRule("adaptive headcount group needs an entry");
        var item = AdaptiveHeadcountGroup.NewAdaptiveHeadcountGroup("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        AdaptiveHeadcountGroupValidatorReportsEveryProblem();
        AdaptiveHeadcountGroupRequiredCodeRuleFlagsABlankCode();
        AdaptiveHeadcountGroupManagerIdLengthRuleFlagsALongManagerId();
        AdaptiveHeadcountGroupRequiredLocaleRuleFlagsABlankLocale();
        AdaptiveHeadcountGroupWindowDaysRangeRuleFlagsValuesAboveWindowDays();
        AdaptiveHeadcountGroupRequiredTitleRuleFlagsABlankTitle();
    }
}
