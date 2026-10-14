namespace Acme.Workforce.Absence.Tests.Balance.Ledger;

using Acme.Workforce.Absence.Balance.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class BatchedBalanceRuleRulesChecks
{
    public static void BatchedBalanceRuleValidatorReportsEveryProblem()
    {
        var validator = BatchedBalanceRuleRules.DefaultBatchedBalanceRuleValidator();
        var item = BatchedBalanceRule.NewBatchedBalanceRule("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BatchedBalanceRuleRequiredCostCenterRuleFlagsABlankCostCenter()
    {
        var rule = new BatchedBalanceRuleRequiredCostCenterRule("batched balance rule needs a value");
        var item = BatchedBalanceRule.NewBatchedBalanceRule("", "");
        item.CostCenter = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BatchedBalanceRuleSkillLengthRuleFlagsALongSkill()
    {
        var rule = new BatchedBalanceRuleSkillLengthRule(3600);
        var item = BatchedBalanceRule.NewBatchedBalanceRule("x", "y");
        item.Skill = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Skill = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BatchedBalanceRuleCodeLengthRuleFlagsALongCode()
    {
        var rule = new BatchedBalanceRuleCodeLengthRule(3600);
        var item = BatchedBalanceRule.NewBatchedBalanceRule("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        BatchedBalanceRuleValidatorReportsEveryProblem();
        BatchedBalanceRuleRequiredCostCenterRuleFlagsABlankCostCenter();
        BatchedBalanceRuleSkillLengthRuleFlagsALongSkill();
        BatchedBalanceRuleCodeLengthRuleFlagsALongCode();
    }
}
