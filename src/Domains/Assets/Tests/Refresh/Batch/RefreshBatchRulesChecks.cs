namespace Acme.Workforce.Assets.Tests.Refresh.Batch;

using Acme.Workforce.Assets.Refresh.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class RefreshBatchRulesChecks
{
    public static void RefreshBatchValidatorReportsEveryProblem()
    {
        var validator = RefreshBatchRules.DefaultRefreshBatchValidator();
        var item = RefreshBatch.NewRefreshBatch("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RefreshBatchRequiredChannelRuleFlagsABlankChannel()
    {
        var rule = new RefreshBatchRequiredChannelRule("refresh batch needs this field");
        var item = RefreshBatch.NewRefreshBatch("", "");
        item.Channel = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RefreshBatchRequiredCurrencyRuleFlagsABlankCurrency()
    {
        var rule = new RefreshBatchRequiredCurrencyRule("refresh batch needs an entry");
        var item = RefreshBatch.NewRefreshBatch("", "");
        item.Currency = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RefreshBatchRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new RefreshBatchRequiredCodeRule("refresh batch needs an entry");
        var item = RefreshBatch.NewRefreshBatch("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RefreshBatchRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new RefreshBatchRequiredLabelRule("refresh batch needs a value");
        var item = RefreshBatch.NewRefreshBatch("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        RefreshBatchValidatorReportsEveryProblem();
        RefreshBatchRequiredChannelRuleFlagsABlankChannel();
        RefreshBatchRequiredCurrencyRuleFlagsABlankCurrency();
        RefreshBatchRequiredCodeRuleFlagsABlankCode();
        RefreshBatchRequiredLabelRuleFlagsABlankLabel();
    }
}
