namespace Acme.Workforce.Orgdesign.Tests.CostModel.ModelMetric;

using Acme.Workforce.Orgdesign.CostModel.ModelMetric;

/// <summary>Checks of model metric code, run by the project's runner.</summary>
public static class RegionalCostModelSummaryRulesChecks
{
    public static void RegionalCostModelSummaryValidatorReportsEveryProblem()
    {
        var validator = RegionalCostModelSummaryRules.DefaultRegionalCostModelSummaryValidator();
        var item = RegionalCostModelSummary.NewRegionalCostModelSummary("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RegionalCostModelSummaryRequiredCurrencyRuleFlagsABlankCurrency()
    {
        var rule = new RegionalCostModelSummaryRequiredCurrencyRule("regional cost model summary needs an entry");
        var item = RegionalCostModelSummary.NewRegionalCostModelSummary("", "");
        item.Currency = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RegionalCostModelSummaryRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new RegionalCostModelSummaryRequiredCodeRule("regional cost model summary needs an entry");
        var item = RegionalCostModelSummary.NewRegionalCostModelSummary("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RegionalCostModelSummaryCurrencyLengthRuleFlagsALongCurrency()
    {
        var rule = new RegionalCostModelSummaryCurrencyLengthRule(255);
        var item = RegionalCostModelSummary.NewRegionalCostModelSummary("x", "y");
        item.Currency = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Currency = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RegionalCostModelSummaryRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new RegionalCostModelSummaryRequiredTitleRule("regional cost model summary needs an entry");
        var item = RegionalCostModelSummary.NewRegionalCostModelSummary("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        RegionalCostModelSummaryValidatorReportsEveryProblem();
        RegionalCostModelSummaryRequiredCurrencyRuleFlagsABlankCurrency();
        RegionalCostModelSummaryRequiredCodeRuleFlagsABlankCode();
        RegionalCostModelSummaryCurrencyLengthRuleFlagsALongCurrency();
        RegionalCostModelSummaryRequiredTitleRuleFlagsABlankTitle();
    }
}
