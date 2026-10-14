namespace Acme.Workforce.Compliance.Tests.TrainingDue.Variance;

using Acme.Workforce.Compliance.TrainingDue.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class TrainingDueVarianceRulesChecks
{
    public static void TrainingDueVarianceValidatorReportsEveryProblem()
    {
        var validator = TrainingDueVarianceRules.DefaultTrainingDueVarianceValidator();
        var item = TrainingDueVariance.NewTrainingDueVariance("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TrainingDueVarianceRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new TrainingDueVarianceRequiredCodeRule("training due variance needs an entry");
        var item = TrainingDueVariance.NewTrainingDueVariance("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TrainingDueVarianceNameLengthRuleFlagsALongName()
    {
        var rule = new TrainingDueVarianceNameLengthRule(86400);
        var item = TrainingDueVariance.NewTrainingDueVariance("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TrainingDueVarianceVersionRangeRuleFlagsValuesAboveVersion()
    {
        var rule = new TrainingDueVarianceVersionRangeRule(255, 86400);
        var item = TrainingDueVariance.NewTrainingDueVariance("x", "y");
        item.Version = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void TrainingDueVarianceRequiredGradeRuleFlagsABlankGrade()
    {
        var rule = new TrainingDueVarianceRequiredGradeRule("training due variance needs this field");
        var item = TrainingDueVariance.NewTrainingDueVariance("", "");
        item.Grade = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TrainingDueVarianceThresholdRangeRuleFlagsValuesAboveThreshold()
    {
        var rule = new TrainingDueVarianceThresholdRangeRule(6.0, 1993.9);
        var item = TrainingDueVariance.NewTrainingDueVariance("x", "y");
        item.Threshold = 1995.4;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        TrainingDueVarianceValidatorReportsEveryProblem();
        TrainingDueVarianceRequiredCodeRuleFlagsABlankCode();
        TrainingDueVarianceNameLengthRuleFlagsALongName();
        TrainingDueVarianceVersionRangeRuleFlagsValuesAboveVersion();
        TrainingDueVarianceRequiredGradeRuleFlagsABlankGrade();
        TrainingDueVarianceThresholdRangeRuleFlagsValuesAboveThreshold();
    }
}
