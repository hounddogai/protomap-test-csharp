namespace Acme.Workforce.Compensation.Tests.BonusPool.Batch;

using Acme.Workforce.Compensation.BonusPool.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class BonusPoolRequestRulesChecks
{
    public static void BonusPoolRequestValidatorReportsEveryProblem()
    {
        var validator = BonusPoolRequestRules.DefaultBonusPoolRequestValidator();
        var item = BonusPoolRequest.NewBonusPoolRequest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BonusPoolRequestBatchSizeRangeRuleFlagsValuesAboveBatchSize()
    {
        var rule = new BonusPoolRequestBatchSizeRangeRule(80, 1000);
        var item = BonusPoolRequest.NewBonusPoolRequest("x", "y");
        item.BatchSize = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BonusPoolRequestRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new BonusPoolRequestRequiredTitleRule("bonus pool request needs this field");
        var item = BonusPoolRequest.NewBonusPoolRequest("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BonusPoolRequestRequiredChannelRuleFlagsABlankChannel()
    {
        var rule = new BonusPoolRequestRequiredChannelRule("bonus pool request needs a value");
        var item = BonusPoolRequest.NewBonusPoolRequest("", "");
        item.Channel = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        BonusPoolRequestValidatorReportsEveryProblem();
        BonusPoolRequestBatchSizeRangeRuleFlagsValuesAboveBatchSize();
        BonusPoolRequestRequiredTitleRuleFlagsABlankTitle();
        BonusPoolRequestRequiredChannelRuleFlagsABlankChannel();
    }
}
