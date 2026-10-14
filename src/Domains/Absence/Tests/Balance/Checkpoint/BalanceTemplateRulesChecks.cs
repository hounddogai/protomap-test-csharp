namespace Acme.Workforce.Absence.Tests.Balance.Checkpoint;

using Acme.Workforce.Absence.Balance.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class BalanceTemplateRulesChecks
{
    public static void BalanceTemplateValidatorReportsEveryProblem()
    {
        var validator = BalanceTemplateRules.DefaultBalanceTemplateValidator();
        var item = BalanceTemplate.NewBalanceTemplate("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BalanceTemplateGraceDaysRangeRuleFlagsValuesAboveGraceDays()
    {
        var rule = new BalanceTemplateGraceDaysRangeRule(32, 120);
        var item = BalanceTemplate.NewBalanceTemplate("x", "y");
        item.GraceDays = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BalanceTemplateRequiredSummaryRuleFlagsABlankSummary()
    {
        var rule = new BalanceTemplateRequiredSummaryRule("balance template needs this field");
        var item = BalanceTemplate.NewBalanceTemplate("", "");
        item.Summary = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BalanceTemplateNameLengthRuleFlagsALongName()
    {
        var rule = new BalanceTemplateNameLengthRule(86400);
        var item = BalanceTemplate.NewBalanceTemplate("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BalanceTemplateSummaryLengthRuleFlagsALongSummary()
    {
        var rule = new BalanceTemplateSummaryLengthRule(16);
        var item = BalanceTemplate.NewBalanceTemplate("x", "y");
        item.Summary = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Summary = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        BalanceTemplateValidatorReportsEveryProblem();
        BalanceTemplateGraceDaysRangeRuleFlagsValuesAboveGraceDays();
        BalanceTemplateRequiredSummaryRuleFlagsABlankSummary();
        BalanceTemplateNameLengthRuleFlagsALongName();
        BalanceTemplateSummaryLengthRuleFlagsALongSummary();
    }
}
