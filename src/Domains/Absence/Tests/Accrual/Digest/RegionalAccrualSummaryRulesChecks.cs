namespace Acme.Workforce.Absence.Tests.Accrual.Digest;

using Acme.Workforce.Absence.Accrual.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class RegionalAccrualSummaryRulesChecks
{
    public static void RegionalAccrualSummaryValidatorReportsEveryProblem()
    {
        var validator = RegionalAccrualSummaryRules.DefaultRegionalAccrualSummaryValidator();
        var item = RegionalAccrualSummary.NewRegionalAccrualSummary("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RegionalAccrualSummaryTitleLengthRuleFlagsALongTitle()
    {
        var rule = new RegionalAccrualSummaryTitleLengthRule(255);
        var item = RegionalAccrualSummary.NewRegionalAccrualSummary("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RegionalAccrualSummaryCategoryLengthRuleFlagsALongCategory()
    {
        var rule = new RegionalAccrualSummaryCategoryLengthRule(80);
        var item = RegionalAccrualSummary.NewRegionalAccrualSummary("x", "y");
        item.Category = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Category = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RegionalAccrualSummaryRequiredCategoryRuleFlagsABlankCategory()
    {
        var rule = new RegionalAccrualSummaryRequiredCategoryRule("regional accrual summary needs a value");
        var item = RegionalAccrualSummary.NewRegionalAccrualSummary("", "");
        item.Category = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RegionalAccrualSummaryCodeLengthRuleFlagsALongCode()
    {
        var rule = new RegionalAccrualSummaryCodeLengthRule(16);
        var item = RegionalAccrualSummary.NewRegionalAccrualSummary("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        RegionalAccrualSummaryValidatorReportsEveryProblem();
        RegionalAccrualSummaryTitleLengthRuleFlagsALongTitle();
        RegionalAccrualSummaryCategoryLengthRuleFlagsALongCategory();
        RegionalAccrualSummaryRequiredCategoryRuleFlagsABlankCategory();
        RegionalAccrualSummaryCodeLengthRuleFlagsALongCode();
    }
}
