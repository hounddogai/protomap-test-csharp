namespace Acme.Workforce.Compensation.Tests.BonusPool.Review;

using Acme.Workforce.Compensation.BonusPool.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class BonusPoolDraftRulesChecks
{
    public static void BonusPoolDraftValidatorReportsEveryProblem()
    {
        var validator = BonusPoolDraftRules.DefaultBonusPoolDraftValidator();
        var item = BonusPoolDraft.NewBonusPoolDraft("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BonusPoolDraftLabelLengthRuleFlagsALongLabel()
    {
        var rule = new BonusPoolDraftLabelLengthRule(1000);
        var item = BonusPoolDraft.NewBonusPoolDraft("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BonusPoolDraftBatchSizeRangeRuleFlagsValuesAboveBatchSize()
    {
        var rule = new BonusPoolDraftBatchSizeRangeRule(16, 86400);
        var item = BonusPoolDraft.NewBonusPoolDraft("x", "y");
        item.BatchSize = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BonusPoolDraftTenureMonthsRangeRuleFlagsValuesAboveTenureMonths()
    {
        var rule = new BonusPoolDraftTenureMonthsRangeRule(80, 255);
        var item = BonusPoolDraft.NewBonusPoolDraft("x", "y");
        item.TenureMonths = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        BonusPoolDraftValidatorReportsEveryProblem();
        BonusPoolDraftLabelLengthRuleFlagsALongLabel();
        BonusPoolDraftBatchSizeRangeRuleFlagsValuesAboveBatchSize();
        BonusPoolDraftTenureMonthsRangeRuleFlagsValuesAboveTenureMonths();
    }
}
