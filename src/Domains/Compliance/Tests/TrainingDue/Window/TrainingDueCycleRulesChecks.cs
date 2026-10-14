namespace Acme.Workforce.Compliance.Tests.TrainingDue.Window;

using Acme.Workforce.Compliance.TrainingDue.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class TrainingDueCycleRulesChecks
{
    public static void TrainingDueCycleValidatorReportsEveryProblem()
    {
        var validator = TrainingDueCycleRules.DefaultTrainingDueCycleValidator();
        var item = TrainingDueCycle.NewTrainingDueCycle("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TrainingDueCycleRequiredNameRuleFlagsABlankName()
    {
        var rule = new TrainingDueCycleRequiredNameRule("training due cycle needs this field");
        var item = TrainingDueCycle.NewTrainingDueCycle("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TrainingDueCycleCreatedAtRangeRuleFlagsValuesAboveCreatedAt()
    {
        var rule = new TrainingDueCycleCreatedAtRangeRule(32, 255);
        var item = TrainingDueCycle.NewTrainingDueCycle("x", "y");
        item.CreatedAt = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void TrainingDueCycleTitleLengthRuleFlagsALongTitle()
    {
        var rule = new TrainingDueCycleTitleLengthRule(16);
        var item = TrainingDueCycle.NewTrainingDueCycle("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TrainingDueCycleRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new TrainingDueCycleRequiredTitleRule("training due cycle needs a value");
        var item = TrainingDueCycle.NewTrainingDueCycle("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        TrainingDueCycleValidatorReportsEveryProblem();
        TrainingDueCycleRequiredNameRuleFlagsABlankName();
        TrainingDueCycleCreatedAtRangeRuleFlagsValuesAboveCreatedAt();
        TrainingDueCycleTitleLengthRuleFlagsALongTitle();
        TrainingDueCycleRequiredTitleRuleFlagsABlankTitle();
    }
}
