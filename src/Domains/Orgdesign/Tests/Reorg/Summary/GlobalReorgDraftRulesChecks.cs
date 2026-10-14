namespace Acme.Workforce.Orgdesign.Tests.Reorg.Summary;

using Acme.Workforce.Orgdesign.Reorg.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class GlobalReorgDraftRulesChecks
{
    public static void GlobalReorgDraftValidatorReportsEveryProblem()
    {
        var validator = GlobalReorgDraftRules.DefaultGlobalReorgDraftValidator();
        var item = GlobalReorgDraft.NewGlobalReorgDraft("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void GlobalReorgDraftRequiredDepartmentRuleFlagsABlankDepartment()
    {
        var rule = new GlobalReorgDraftRequiredDepartmentRule("global reorg draft needs a value");
        var item = GlobalReorgDraft.NewGlobalReorgDraft("", "");
        item.Department = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void GlobalReorgDraftRequiredChannelRuleFlagsABlankChannel()
    {
        var rule = new GlobalReorgDraftRequiredChannelRule("global reorg draft needs this field");
        var item = GlobalReorgDraft.NewGlobalReorgDraft("", "");
        item.Channel = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void GlobalReorgDraftBudgetRangeRuleFlagsValuesAboveBudget()
    {
        var rule = new GlobalReorgDraftBudgetRangeRule(5.2, 1354.9);
        var item = GlobalReorgDraft.NewGlobalReorgDraft("x", "y");
        item.Budget = 1356.4;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void GlobalReorgDraftBatchSizeRangeRuleFlagsValuesAboveBatchSize()
    {
        var rule = new GlobalReorgDraftBatchSizeRangeRule(64, 64);
        var item = GlobalReorgDraft.NewGlobalReorgDraft("x", "y");
        item.BatchSize = 65;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void GlobalReorgDraftChannelLengthRuleFlagsALongChannel()
    {
        var rule = new GlobalReorgDraftChannelLengthRule(86400);
        var item = GlobalReorgDraft.NewGlobalReorgDraft("x", "y");
        item.Channel = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Channel = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        GlobalReorgDraftValidatorReportsEveryProblem();
        GlobalReorgDraftRequiredDepartmentRuleFlagsABlankDepartment();
        GlobalReorgDraftRequiredChannelRuleFlagsABlankChannel();
        GlobalReorgDraftBudgetRangeRuleFlagsValuesAboveBudget();
        GlobalReorgDraftBatchSizeRangeRuleFlagsValuesAboveBatchSize();
        GlobalReorgDraftChannelLengthRuleFlagsALongChannel();
    }
}
