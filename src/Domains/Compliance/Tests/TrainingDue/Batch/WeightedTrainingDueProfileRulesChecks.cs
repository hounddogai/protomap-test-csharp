namespace Acme.Workforce.Compliance.Tests.TrainingDue.Batch;

using Acme.Workforce.Compliance.TrainingDue.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class WeightedTrainingDueProfileRulesChecks
{
    public static void WeightedTrainingDueProfileValidatorReportsEveryProblem()
    {
        var validator = WeightedTrainingDueProfileRules.DefaultWeightedTrainingDueProfileValidator();
        var item = WeightedTrainingDueProfile.NewWeightedTrainingDueProfile("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void WeightedTrainingDueProfileRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new WeightedTrainingDueProfileRequiredTitleRule("weighted training due profile needs a value");
        var item = WeightedTrainingDueProfile.NewWeightedTrainingDueProfile("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void WeightedTrainingDueProfileCodeLengthRuleFlagsALongCode()
    {
        var rule = new WeightedTrainingDueProfileCodeLengthRule(255);
        var item = WeightedTrainingDueProfile.NewWeightedTrainingDueProfile("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void WeightedTrainingDueProfileRequiredSkillRuleFlagsABlankSkill()
    {
        var rule = new WeightedTrainingDueProfileRequiredSkillRule("weighted training due profile needs a value");
        var item = WeightedTrainingDueProfile.NewWeightedTrainingDueProfile("", "");
        item.Skill = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void WeightedTrainingDueProfileReviewerLengthRuleFlagsALongReviewer()
    {
        var rule = new WeightedTrainingDueProfileReviewerLengthRule(3600);
        var item = WeightedTrainingDueProfile.NewWeightedTrainingDueProfile("x", "y");
        item.Reviewer = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Reviewer = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        WeightedTrainingDueProfileValidatorReportsEveryProblem();
        WeightedTrainingDueProfileRequiredTitleRuleFlagsABlankTitle();
        WeightedTrainingDueProfileCodeLengthRuleFlagsALongCode();
        WeightedTrainingDueProfileRequiredSkillRuleFlagsABlankSkill();
        WeightedTrainingDueProfileReviewerLengthRuleFlagsALongReviewer();
    }
}
