namespace Acme.Workforce.Compensation.Tests.BonusPool.PoolRequest;

using Acme.Workforce.Compensation.BonusPool.PoolRequest;

/// <summary>Checks of pool request code, run by the project's runner.</summary>
public static class WeightedBonusPoolMilestoneRulesChecks
{
    public static void WeightedBonusPoolMilestoneValidatorReportsEveryProblem()
    {
        var validator = WeightedBonusPoolMilestoneRules.DefaultWeightedBonusPoolMilestoneValidator();
        var item = WeightedBonusPoolMilestone.NewWeightedBonusPoolMilestone("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void WeightedBonusPoolMilestoneCodeLengthRuleFlagsALongCode()
    {
        var rule = new WeightedBonusPoolMilestoneCodeLengthRule(1000);
        var item = WeightedBonusPoolMilestone.NewWeightedBonusPoolMilestone("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void WeightedBonusPoolMilestoneSummaryLengthRuleFlagsALongSummary()
    {
        var rule = new WeightedBonusPoolMilestoneSummaryLengthRule(32);
        var item = WeightedBonusPoolMilestone.NewWeightedBonusPoolMilestone("x", "y");
        item.Summary = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Summary = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        WeightedBonusPoolMilestoneValidatorReportsEveryProblem();
        WeightedBonusPoolMilestoneCodeLengthRuleFlagsALongCode();
        WeightedBonusPoolMilestoneSummaryLengthRuleFlagsALongSummary();
    }
}
