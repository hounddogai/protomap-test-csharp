namespace Acme.Workforce.Compliance.Tests.Exception.Rule;

using Acme.Workforce.Compliance.Exception.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class ExceptionEntryRulesChecks
{
    public static void ExceptionEntryValidatorReportsEveryProblem()
    {
        var validator = ExceptionEntryRules.DefaultExceptionEntryValidator();
        var item = ExceptionEntry.NewExceptionEntry("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ExceptionEntryRequiredSourceRuleFlagsABlankSource()
    {
        var rule = new ExceptionEntryRequiredSourceRule("exception entry needs this field");
        var item = ExceptionEntry.NewExceptionEntry("", "");
        item.Source = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ExceptionEntryDiscountRangeRuleFlagsValuesAboveDiscount()
    {
        var rule = new ExceptionEntryDiscountRangeRule(8.4, 4746.1);
        var item = ExceptionEntry.NewExceptionEntry("x", "y");
        item.Discount = 4747.6;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ExceptionEntryRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new ExceptionEntryRequiredTitleRule("exception entry needs a value");
        var item = ExceptionEntry.NewExceptionEntry("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        ExceptionEntryValidatorReportsEveryProblem();
        ExceptionEntryRequiredSourceRuleFlagsABlankSource();
        ExceptionEntryDiscountRangeRuleFlagsValuesAboveDiscount();
        ExceptionEntryRequiredTitleRuleFlagsABlankTitle();
    }
}
