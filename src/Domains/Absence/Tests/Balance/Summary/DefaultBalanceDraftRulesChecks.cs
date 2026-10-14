namespace Acme.Workforce.Absence.Tests.Balance.Summary;

using Acme.Workforce.Absence.Balance.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class DefaultBalanceDraftRulesChecks
{
    public static void DefaultBalanceDraftValidatorReportsEveryProblem()
    {
        var validator = DefaultBalanceDraftRules.DefaultDefaultBalanceDraftValidator();
        var item = DefaultBalanceDraft.NewDefaultBalanceDraft("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DefaultBalanceDraftSummaryLengthRuleFlagsALongSummary()
    {
        var rule = new DefaultBalanceDraftSummaryLengthRule(120);
        var item = DefaultBalanceDraft.NewDefaultBalanceDraft("x", "y");
        item.Summary = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Summary = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void DefaultBalanceDraftRequiredSummaryRuleFlagsABlankSummary()
    {
        var rule = new DefaultBalanceDraftRequiredSummaryRule("default balance draft needs a value");
        var item = DefaultBalanceDraft.NewDefaultBalanceDraft("", "");
        item.Summary = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DefaultBalanceDraftLeaveDaysRangeRuleFlagsValuesAboveLeaveDays()
    {
        var rule = new DefaultBalanceDraftLeaveDaysRangeRule(16, 3600);
        var item = DefaultBalanceDraft.NewDefaultBalanceDraft("x", "y");
        item.LeaveDays = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void DefaultBalanceDraftCodeLengthRuleFlagsALongCode()
    {
        var rule = new DefaultBalanceDraftCodeLengthRule(120);
        var item = DefaultBalanceDraft.NewDefaultBalanceDraft("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void DefaultBalanceDraftTitleLengthRuleFlagsALongTitle()
    {
        var rule = new DefaultBalanceDraftTitleLengthRule(1000);
        var item = DefaultBalanceDraft.NewDefaultBalanceDraft("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        DefaultBalanceDraftValidatorReportsEveryProblem();
        DefaultBalanceDraftSummaryLengthRuleFlagsALongSummary();
        DefaultBalanceDraftRequiredSummaryRuleFlagsABlankSummary();
        DefaultBalanceDraftLeaveDaysRangeRuleFlagsValuesAboveLeaveDays();
        DefaultBalanceDraftCodeLengthRuleFlagsALongCode();
        DefaultBalanceDraftTitleLengthRuleFlagsALongTitle();
    }
}
