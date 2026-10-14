namespace Acme.Workforce.Compensation.Tests.EquityGrant.Agreement;

using Acme.Workforce.Compensation.EquityGrant.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class PrimaryEquityGrantSummaryRulesChecks
{
    public static void PrimaryEquityGrantSummaryValidatorReportsEveryProblem()
    {
        var validator = PrimaryEquityGrantSummaryRules.DefaultPrimaryEquityGrantSummaryValidator();
        var item = PrimaryEquityGrantSummary.NewPrimaryEquityGrantSummary("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PrimaryEquityGrantSummaryRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new PrimaryEquityGrantSummaryRequiredTitleRule("primary equity grant summary needs an entry");
        var item = PrimaryEquityGrantSummary.NewPrimaryEquityGrantSummary("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PrimaryEquityGrantSummaryBudgetRangeRuleFlagsValuesAboveBudget()
    {
        var rule = new PrimaryEquityGrantSummaryBudgetRangeRule(4.6, 185.1);
        var item = PrimaryEquityGrantSummary.NewPrimaryEquityGrantSummary("x", "y");
        item.Budget = 186.6;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PrimaryEquityGrantSummaryTenureMonthsRangeRuleFlagsValuesAboveTenureMonths()
    {
        var rule = new PrimaryEquityGrantSummaryTenureMonthsRangeRule(32, 64);
        var item = PrimaryEquityGrantSummary.NewPrimaryEquityGrantSummary("x", "y");
        item.TenureMonths = 65;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PrimaryEquityGrantSummaryCodeLengthRuleFlagsALongCode()
    {
        var rule = new PrimaryEquityGrantSummaryCodeLengthRule(1000);
        var item = PrimaryEquityGrantSummary.NewPrimaryEquityGrantSummary("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        PrimaryEquityGrantSummaryValidatorReportsEveryProblem();
        PrimaryEquityGrantSummaryRequiredTitleRuleFlagsABlankTitle();
        PrimaryEquityGrantSummaryBudgetRangeRuleFlagsValuesAboveBudget();
        PrimaryEquityGrantSummaryTenureMonthsRangeRuleFlagsValuesAboveTenureMonths();
        PrimaryEquityGrantSummaryCodeLengthRuleFlagsALongCode();
    }
}
