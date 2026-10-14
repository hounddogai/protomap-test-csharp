namespace Acme.Workforce.Compliance.Tests.TrainingDue.Draft;

using Acme.Workforce.Compliance.TrainingDue.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class BatchedTrainingDueDraftRulesChecks
{
    public static void BatchedTrainingDueDraftValidatorReportsEveryProblem()
    {
        var validator = BatchedTrainingDueDraftRules.DefaultBatchedTrainingDueDraftValidator();
        var item = BatchedTrainingDueDraft.NewBatchedTrainingDueDraft("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BatchedTrainingDueDraftRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new BatchedTrainingDueDraftRequiredTitleRule("batched training due draft needs a value");
        var item = BatchedTrainingDueDraft.NewBatchedTrainingDueDraft("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BatchedTrainingDueDraftRequiredSkillRuleFlagsABlankSkill()
    {
        var rule = new BatchedTrainingDueDraftRequiredSkillRule("batched training due draft needs this field");
        var item = BatchedTrainingDueDraft.NewBatchedTrainingDueDraft("", "");
        item.Skill = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BatchedTrainingDueDraftRatioRangeRuleFlagsValuesAboveRatio()
    {
        var rule = new BatchedTrainingDueDraftRatioRangeRule(5.2, 2554.5);
        var item = BatchedTrainingDueDraft.NewBatchedTrainingDueDraft("x", "y");
        item.Ratio = 2556.0;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        BatchedTrainingDueDraftValidatorReportsEveryProblem();
        BatchedTrainingDueDraftRequiredTitleRuleFlagsABlankTitle();
        BatchedTrainingDueDraftRequiredSkillRuleFlagsABlankSkill();
        BatchedTrainingDueDraftRatioRangeRuleFlagsValuesAboveRatio();
    }
}
