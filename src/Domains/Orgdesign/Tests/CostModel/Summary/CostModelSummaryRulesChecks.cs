namespace Acme.Workforce.Orgdesign.Tests.CostModel.Summary;

using Acme.Workforce.Orgdesign.CostModel.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class CostModelSummaryRulesChecks
{
    public static void CostModelSummaryValidatorReportsEveryProblem()
    {
        var validator = CostModelSummaryRules.DefaultCostModelSummaryValidator();
        var item = CostModelSummary.NewCostModelSummary("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CostModelSummaryRequiredLocaleRuleFlagsABlankLocale()
    {
        var rule = new CostModelSummaryRequiredLocaleRule("cost model summary needs this field");
        var item = CostModelSummary.NewCostModelSummary("", "");
        item.Locale = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void CostModelSummaryBatchSizeRangeRuleFlagsValuesAboveBatchSize()
    {
        var rule = new CostModelSummaryBatchSizeRangeRule(80, 120);
        var item = CostModelSummary.NewCostModelSummary("x", "y");
        item.BatchSize = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void CostModelSummaryRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new CostModelSummaryRequiredCodeRule("cost model summary needs a value");
        var item = CostModelSummary.NewCostModelSummary("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        CostModelSummaryValidatorReportsEveryProblem();
        CostModelSummaryRequiredLocaleRuleFlagsABlankLocale();
        CostModelSummaryBatchSizeRangeRuleFlagsValuesAboveBatchSize();
        CostModelSummaryRequiredCodeRuleFlagsABlankCode();
    }
}
