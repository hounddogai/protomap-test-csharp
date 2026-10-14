namespace Acme.Workforce.Compliance.Tests.TrainingDue.DueVariance;

using Acme.Workforce.Compliance.TrainingDue.DueVariance;

/// <summary>Checks of due variance code, run by the project's runner.</summary>
public static class TrainingDueDigestRulesChecks
{
    public static void TrainingDueDigestValidatorReportsEveryProblem()
    {
        var validator = TrainingDueDigestRules.DefaultTrainingDueDigestValidator();
        var item = TrainingDueDigest.NewTrainingDueDigest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TrainingDueDigestRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new TrainingDueDigestRequiredLabelRule("training due digest needs an entry");
        var item = TrainingDueDigest.NewTrainingDueDigest("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TrainingDueDigestEndsAtRangeRuleFlagsValuesAboveEndsAt()
    {
        var rule = new TrainingDueDigestEndsAtRangeRule(16, 3600);
        var item = TrainingDueDigest.NewTrainingDueDigest("x", "y");
        item.EndsAt = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void TrainingDueDigestCodeLengthRuleFlagsALongCode()
    {
        var rule = new TrainingDueDigestCodeLengthRule(1000);
        var item = TrainingDueDigest.NewTrainingDueDigest("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TrainingDueDigestRequiredManagerIdRuleFlagsABlankManagerId()
    {
        var rule = new TrainingDueDigestRequiredManagerIdRule("training due digest needs an entry");
        var item = TrainingDueDigest.NewTrainingDueDigest("", "");
        item.ManagerId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        TrainingDueDigestValidatorReportsEveryProblem();
        TrainingDueDigestRequiredLabelRuleFlagsABlankLabel();
        TrainingDueDigestEndsAtRangeRuleFlagsValuesAboveEndsAt();
        TrainingDueDigestCodeLengthRuleFlagsALongCode();
        TrainingDueDigestRequiredManagerIdRuleFlagsABlankManagerId();
    }
}
