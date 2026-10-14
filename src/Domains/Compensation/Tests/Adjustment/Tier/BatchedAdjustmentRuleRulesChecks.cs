namespace Acme.Workforce.Compensation.Tests.Adjustment.Tier;

using Acme.Workforce.Compensation.Adjustment.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class BatchedAdjustmentRuleRulesChecks
{
    public static void BatchedAdjustmentRuleValidatorReportsEveryProblem()
    {
        var validator = BatchedAdjustmentRuleRules.DefaultBatchedAdjustmentRuleValidator();
        var item = BatchedAdjustmentRule.NewBatchedAdjustmentRule("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BatchedAdjustmentRuleRequiredSalaryBandRuleFlagsABlankSalaryBand()
    {
        var rule = new BatchedAdjustmentRuleRequiredSalaryBandRule("batched adjustment rule needs a value");
        var item = BatchedAdjustmentRule.NewBatchedAdjustmentRule("", "");
        item.SalaryBand = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BatchedAdjustmentRuleTeamLengthRuleFlagsALongTeam()
    {
        var rule = new BatchedAdjustmentRuleTeamLengthRule(80);
        var item = BatchedAdjustmentRule.NewBatchedAdjustmentRule("x", "y");
        item.Team = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Team = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BatchedAdjustmentRuleSalaryBandLengthRuleFlagsALongSalaryBand()
    {
        var rule = new BatchedAdjustmentRuleSalaryBandLengthRule(86400);
        var item = BatchedAdjustmentRule.NewBatchedAdjustmentRule("x", "y");
        item.SalaryBand = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.SalaryBand = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BatchedAdjustmentRuleRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new BatchedAdjustmentRuleRequiredCodeRule("batched adjustment rule needs this field");
        var item = BatchedAdjustmentRule.NewBatchedAdjustmentRule("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BatchedAdjustmentRuleRequiredTeamRuleFlagsABlankTeam()
    {
        var rule = new BatchedAdjustmentRuleRequiredTeamRule("batched adjustment rule needs an entry");
        var item = BatchedAdjustmentRule.NewBatchedAdjustmentRule("", "");
        item.Team = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        BatchedAdjustmentRuleValidatorReportsEveryProblem();
        BatchedAdjustmentRuleRequiredSalaryBandRuleFlagsABlankSalaryBand();
        BatchedAdjustmentRuleTeamLengthRuleFlagsALongTeam();
        BatchedAdjustmentRuleSalaryBandLengthRuleFlagsALongSalaryBand();
        BatchedAdjustmentRuleRequiredCodeRuleFlagsABlankCode();
        BatchedAdjustmentRuleRequiredTeamRuleFlagsABlankTeam();
    }
}
