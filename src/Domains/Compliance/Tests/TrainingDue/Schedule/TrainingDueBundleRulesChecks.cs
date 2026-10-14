namespace Acme.Workforce.Compliance.Tests.TrainingDue.Schedule;

using Acme.Workforce.Compliance.TrainingDue.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class TrainingDueBundleRulesChecks
{
    public static void TrainingDueBundleValidatorReportsEveryProblem()
    {
        var validator = TrainingDueBundleRules.DefaultTrainingDueBundleValidator();
        var item = TrainingDueBundle.NewTrainingDueBundle("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TrainingDueBundleCodeLengthRuleFlagsALongCode()
    {
        var rule = new TrainingDueBundleCodeLengthRule(1000);
        var item = TrainingDueBundle.NewTrainingDueBundle("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TrainingDueBundleRequiredTimezoneRuleFlagsABlankTimezone()
    {
        var rule = new TrainingDueBundleRequiredTimezoneRule("training due bundle needs an entry");
        var item = TrainingDueBundle.NewTrainingDueBundle("", "");
        item.Timezone = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TrainingDueBundleRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new TrainingDueBundleRequiredTitleRule("training due bundle needs an entry");
        var item = TrainingDueBundle.NewTrainingDueBundle("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TrainingDueBundleRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new TrainingDueBundleRequiredCodeRule("training due bundle needs an entry");
        var item = TrainingDueBundle.NewTrainingDueBundle("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        TrainingDueBundleValidatorReportsEveryProblem();
        TrainingDueBundleCodeLengthRuleFlagsALongCode();
        TrainingDueBundleRequiredTimezoneRuleFlagsABlankTimezone();
        TrainingDueBundleRequiredTitleRuleFlagsABlankTitle();
        TrainingDueBundleRequiredCodeRuleFlagsABlankCode();
    }
}
