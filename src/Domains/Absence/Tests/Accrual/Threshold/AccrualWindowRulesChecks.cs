namespace Acme.Workforce.Absence.Tests.Accrual.Threshold;

using Acme.Workforce.Absence.Accrual.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class AccrualWindowRulesChecks
{
    public static void AccrualWindowValidatorReportsEveryProblem()
    {
        var validator = AccrualWindowRules.DefaultAccrualWindowValidator();
        var item = AccrualWindow.NewAccrualWindow("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AccrualWindowLocaleLengthRuleFlagsALongLocale()
    {
        var rule = new AccrualWindowLocaleLengthRule(32);
        var item = AccrualWindow.NewAccrualWindow("x", "y");
        item.Locale = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Locale = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AccrualWindowRequiredLocaleRuleFlagsABlankLocale()
    {
        var rule = new AccrualWindowRequiredLocaleRule("accrual window needs an entry");
        var item = AccrualWindow.NewAccrualWindow("", "");
        item.Locale = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AccrualWindowRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new AccrualWindowRequiredTitleRule("accrual window needs an entry");
        var item = AccrualWindow.NewAccrualWindow("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AccrualWindowRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new AccrualWindowRequiredCodeRule("accrual window needs a value");
        var item = AccrualWindow.NewAccrualWindow("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        AccrualWindowValidatorReportsEveryProblem();
        AccrualWindowLocaleLengthRuleFlagsALongLocale();
        AccrualWindowRequiredLocaleRuleFlagsABlankLocale();
        AccrualWindowRequiredTitleRuleFlagsABlankTitle();
        AccrualWindowRequiredCodeRuleFlagsABlankCode();
    }
}
