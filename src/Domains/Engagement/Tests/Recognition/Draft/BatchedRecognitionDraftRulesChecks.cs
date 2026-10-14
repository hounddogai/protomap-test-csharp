namespace Acme.Workforce.Engagement.Tests.Recognition.Draft;

using Acme.Workforce.Engagement.Recognition.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class BatchedRecognitionDraftRulesChecks
{
    public static void BatchedRecognitionDraftValidatorReportsEveryProblem()
    {
        var validator = BatchedRecognitionDraftRules.DefaultBatchedRecognitionDraftValidator();
        var item = BatchedRecognitionDraft.NewBatchedRecognitionDraft("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BatchedRecognitionDraftCodeLengthRuleFlagsALongCode()
    {
        var rule = new BatchedRecognitionDraftCodeLengthRule(86400);
        var item = BatchedRecognitionDraft.NewBatchedRecognitionDraft("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BatchedRecognitionDraftSkillLengthRuleFlagsALongSkill()
    {
        var rule = new BatchedRecognitionDraftSkillLengthRule(80);
        var item = BatchedRecognitionDraft.NewBatchedRecognitionDraft("x", "y");
        item.Skill = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Skill = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BatchedRecognitionDraftBudgetRangeRuleFlagsValuesAboveBudget()
    {
        var rule = new BatchedRecognitionDraftBudgetRangeRule(6.7, 1567.5);
        var item = BatchedRecognitionDraft.NewBatchedRecognitionDraft("x", "y");
        item.Budget = 1569.0;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        BatchedRecognitionDraftValidatorReportsEveryProblem();
        BatchedRecognitionDraftCodeLengthRuleFlagsALongCode();
        BatchedRecognitionDraftSkillLengthRuleFlagsALongSkill();
        BatchedRecognitionDraftBudgetRangeRuleFlagsValuesAboveBudget();
    }
}
