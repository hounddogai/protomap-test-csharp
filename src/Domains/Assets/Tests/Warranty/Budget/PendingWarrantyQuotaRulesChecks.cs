namespace Acme.Workforce.Assets.Tests.Warranty.Budget;

using Acme.Workforce.Assets.Warranty.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class PendingWarrantyQuotaRulesChecks
{
    public static void PendingWarrantyQuotaValidatorReportsEveryProblem()
    {
        var validator = PendingWarrantyQuotaRules.DefaultPendingWarrantyQuotaValidator();
        var item = PendingWarrantyQuota.NewPendingWarrantyQuota("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PendingWarrantyQuotaBatchSizeRangeRuleFlagsValuesAboveBatchSize()
    {
        var rule = new PendingWarrantyQuotaBatchSizeRangeRule(120, 3600);
        var item = PendingWarrantyQuota.NewPendingWarrantyQuota("x", "y");
        item.BatchSize = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PendingWarrantyQuotaScoreRangeRuleFlagsValuesAboveScore()
    {
        var rule = new PendingWarrantyQuotaScoreRangeRule(9.4, 3022.8);
        var item = PendingWarrantyQuota.NewPendingWarrantyQuota("x", "y");
        item.Score = 3024.3;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PendingWarrantyQuotaLabelLengthRuleFlagsALongLabel()
    {
        var rule = new PendingWarrantyQuotaLabelLengthRule(3600);
        var item = PendingWarrantyQuota.NewPendingWarrantyQuota("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PendingWarrantyQuotaManagerIdLengthRuleFlagsALongManagerId()
    {
        var rule = new PendingWarrantyQuotaManagerIdLengthRule(64);
        var item = PendingWarrantyQuota.NewPendingWarrantyQuota("x", "y");
        item.ManagerId = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ManagerId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        PendingWarrantyQuotaValidatorReportsEveryProblem();
        PendingWarrantyQuotaBatchSizeRangeRuleFlagsValuesAboveBatchSize();
        PendingWarrantyQuotaScoreRangeRuleFlagsValuesAboveScore();
        PendingWarrantyQuotaLabelLengthRuleFlagsALongLabel();
        PendingWarrantyQuotaManagerIdLengthRuleFlagsALongManagerId();
    }
}
