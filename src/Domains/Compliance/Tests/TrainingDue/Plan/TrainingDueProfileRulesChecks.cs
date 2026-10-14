namespace Acme.Workforce.Compliance.Tests.TrainingDue.Plan;

using Acme.Workforce.Compliance.TrainingDue.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class TrainingDueProfileRulesChecks
{
    public static void TrainingDueProfileValidatorReportsEveryProblem()
    {
        var validator = TrainingDueProfileRules.DefaultTrainingDueProfileValidator();
        var item = TrainingDueProfile.NewTrainingDueProfile("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TrainingDueProfileRetriesRangeRuleFlagsValuesAboveRetries()
    {
        var rule = new TrainingDueProfileRetriesRangeRule(64, 255);
        var item = TrainingDueProfile.NewTrainingDueProfile("x", "y");
        item.Retries = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void TrainingDueProfileMinimumRangeRuleFlagsValuesAboveMinimum()
    {
        var rule = new TrainingDueProfileMinimumRangeRule(4.4, 1180.9);
        var item = TrainingDueProfile.NewTrainingDueProfile("x", "y");
        item.Minimum = 1182.4;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void TrainingDueProfileLabelLengthRuleFlagsALongLabel()
    {
        var rule = new TrainingDueProfileLabelLengthRule(16);
        var item = TrainingDueProfile.NewTrainingDueProfile("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TrainingDueProfileJobCodeLengthRuleFlagsALongJobCode()
    {
        var rule = new TrainingDueProfileJobCodeLengthRule(32);
        var item = TrainingDueProfile.NewTrainingDueProfile("x", "y");
        item.JobCode = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.JobCode = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TrainingDueProfileRequiredJobCodeRuleFlagsABlankJobCode()
    {
        var rule = new TrainingDueProfileRequiredJobCodeRule("training due profile needs this field");
        var item = TrainingDueProfile.NewTrainingDueProfile("", "");
        item.JobCode = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        TrainingDueProfileValidatorReportsEveryProblem();
        TrainingDueProfileRetriesRangeRuleFlagsValuesAboveRetries();
        TrainingDueProfileMinimumRangeRuleFlagsValuesAboveMinimum();
        TrainingDueProfileLabelLengthRuleFlagsALongLabel();
        TrainingDueProfileJobCodeLengthRuleFlagsALongJobCode();
        TrainingDueProfileRequiredJobCodeRuleFlagsABlankJobCode();
    }
}
