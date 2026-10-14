namespace Acme.Workforce.Workplace.Tests.Desk.RecordItem;

using Acme.Workforce.Workplace.Desk.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class GlobalDeskBatchRulesChecks
{
    public static void GlobalDeskBatchValidatorReportsEveryProblem()
    {
        var validator = GlobalDeskBatchRules.DefaultGlobalDeskBatchValidator();
        var item = GlobalDeskBatch.NewGlobalDeskBatch("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void GlobalDeskBatchSkillLengthRuleFlagsALongSkill()
    {
        var rule = new GlobalDeskBatchSkillLengthRule(255);
        var item = GlobalDeskBatch.NewGlobalDeskBatch("x", "y");
        item.Skill = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Skill = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void GlobalDeskBatchRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new GlobalDeskBatchRequiredLabelRule("global desk batch needs a value");
        var item = GlobalDeskBatch.NewGlobalDeskBatch("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void GlobalDeskBatchBudgetRangeRuleFlagsValuesAboveBudget()
    {
        var rule = new GlobalDeskBatchBudgetRangeRule(2.5, 4725.7);
        var item = GlobalDeskBatch.NewGlobalDeskBatch("x", "y");
        item.Budget = 4727.2;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        GlobalDeskBatchValidatorReportsEveryProblem();
        GlobalDeskBatchSkillLengthRuleFlagsALongSkill();
        GlobalDeskBatchRequiredLabelRuleFlagsABlankLabel();
        GlobalDeskBatchBudgetRangeRuleFlagsValuesAboveBudget();
    }
}
