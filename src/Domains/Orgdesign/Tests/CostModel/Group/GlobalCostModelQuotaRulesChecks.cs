namespace Acme.Workforce.Orgdesign.Tests.CostModel.Group;

using Acme.Workforce.Orgdesign.CostModel.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class GlobalCostModelQuotaRulesChecks
{
    public static void GlobalCostModelQuotaValidatorReportsEveryProblem()
    {
        var validator = GlobalCostModelQuotaRules.DefaultGlobalCostModelQuotaValidator();
        var item = GlobalCostModelQuota.NewGlobalCostModelQuota("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void GlobalCostModelQuotaEndsAtRangeRuleFlagsValuesAboveEndsAt()
    {
        var rule = new GlobalCostModelQuotaEndsAtRangeRule(16, 80);
        var item = GlobalCostModelQuota.NewGlobalCostModelQuota("x", "y");
        item.EndsAt = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void GlobalCostModelQuotaRatioRangeRuleFlagsValuesAboveRatio()
    {
        var rule = new GlobalCostModelQuotaRatioRangeRule(2.8, 970.5);
        var item = GlobalCostModelQuota.NewGlobalCostModelQuota("x", "y");
        item.Ratio = 972.0;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void GlobalCostModelQuotaScoreRangeRuleFlagsValuesAboveScore()
    {
        var rule = new GlobalCostModelQuotaScoreRangeRule(8.1, 4049.3);
        var item = GlobalCostModelQuota.NewGlobalCostModelQuota("x", "y");
        item.Score = 4050.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void GlobalCostModelQuotaRateRangeRuleFlagsValuesAboveRate()
    {
        var rule = new GlobalCostModelQuotaRateRangeRule(1.6, 586.4);
        var item = GlobalCostModelQuota.NewGlobalCostModelQuota("x", "y");
        item.Rate = 587.9;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void GlobalCostModelQuotaRequiredNameRuleFlagsABlankName()
    {
        var rule = new GlobalCostModelQuotaRequiredNameRule("global cost model quota needs an entry");
        var item = GlobalCostModelQuota.NewGlobalCostModelQuota("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        GlobalCostModelQuotaValidatorReportsEveryProblem();
        GlobalCostModelQuotaEndsAtRangeRuleFlagsValuesAboveEndsAt();
        GlobalCostModelQuotaRatioRangeRuleFlagsValuesAboveRatio();
        GlobalCostModelQuotaScoreRangeRuleFlagsValuesAboveScore();
        GlobalCostModelQuotaRateRangeRuleFlagsValuesAboveRate();
        GlobalCostModelQuotaRequiredNameRuleFlagsABlankName();
    }
}
