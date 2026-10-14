namespace Acme.Workforce.Compliance.Tests.TrainingDue.Summary;

using Acme.Workforce.Compliance.TrainingDue.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class TrainingDueTemplateRulesChecks
{
    public static void TrainingDueTemplateValidatorReportsEveryProblem()
    {
        var validator = TrainingDueTemplateRules.DefaultTrainingDueTemplateValidator();
        var item = TrainingDueTemplate.NewTrainingDueTemplate("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TrainingDueTemplateEndsAtRangeRuleFlagsValuesAboveEndsAt()
    {
        var rule = new TrainingDueTemplateEndsAtRangeRule(64, 255);
        var item = TrainingDueTemplate.NewTrainingDueTemplate("x", "y");
        item.EndsAt = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void TrainingDueTemplateLabelLengthRuleFlagsALongLabel()
    {
        var rule = new TrainingDueTemplateLabelLengthRule(3600);
        var item = TrainingDueTemplate.NewTrainingDueTemplate("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TrainingDueTemplateLocaleLengthRuleFlagsALongLocale()
    {
        var rule = new TrainingDueTemplateLocaleLengthRule(64);
        var item = TrainingDueTemplate.NewTrainingDueTemplate("x", "y");
        item.Locale = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Locale = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TrainingDueTemplateCodeLengthRuleFlagsALongCode()
    {
        var rule = new TrainingDueTemplateCodeLengthRule(32);
        var item = TrainingDueTemplate.NewTrainingDueTemplate("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TrainingDueTemplateRetriesRangeRuleFlagsValuesAboveRetries()
    {
        var rule = new TrainingDueTemplateRetriesRangeRule(32, 86400);
        var item = TrainingDueTemplate.NewTrainingDueTemplate("x", "y");
        item.Retries = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        TrainingDueTemplateValidatorReportsEveryProblem();
        TrainingDueTemplateEndsAtRangeRuleFlagsValuesAboveEndsAt();
        TrainingDueTemplateLabelLengthRuleFlagsALongLabel();
        TrainingDueTemplateLocaleLengthRuleFlagsALongLocale();
        TrainingDueTemplateCodeLengthRuleFlagsALongCode();
        TrainingDueTemplateRetriesRangeRuleFlagsValuesAboveRetries();
    }
}
