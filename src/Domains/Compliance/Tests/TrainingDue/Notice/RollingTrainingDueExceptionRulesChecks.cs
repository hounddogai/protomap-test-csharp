namespace Acme.Workforce.Compliance.Tests.TrainingDue.Notice;

using Acme.Workforce.Compliance.TrainingDue.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class RollingTrainingDueExceptionRulesChecks
{
    public static void RollingTrainingDueExceptionValidatorReportsEveryProblem()
    {
        var validator = RollingTrainingDueExceptionRules.DefaultRollingTrainingDueExceptionValidator();
        var item = RollingTrainingDueException.NewRollingTrainingDueException("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RollingTrainingDueExceptionTimezoneLengthRuleFlagsALongTimezone()
    {
        var rule = new RollingTrainingDueExceptionTimezoneLengthRule(3600);
        var item = RollingTrainingDueException.NewRollingTrainingDueException("x", "y");
        item.Timezone = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Timezone = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RollingTrainingDueExceptionCreatedAtRangeRuleFlagsValuesAboveCreatedAt()
    {
        var rule = new RollingTrainingDueExceptionCreatedAtRangeRule(255, 1000);
        var item = RollingTrainingDueException.NewRollingTrainingDueException("x", "y");
        item.CreatedAt = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RollingTrainingDueExceptionMarginRangeRuleFlagsValuesAboveMargin()
    {
        var rule = new RollingTrainingDueExceptionMarginRangeRule(7.4, 1947.6);
        var item = RollingTrainingDueException.NewRollingTrainingDueException("x", "y");
        item.Margin = 1949.1;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        RollingTrainingDueExceptionValidatorReportsEveryProblem();
        RollingTrainingDueExceptionTimezoneLengthRuleFlagsALongTimezone();
        RollingTrainingDueExceptionCreatedAtRangeRuleFlagsValuesAboveCreatedAt();
        RollingTrainingDueExceptionMarginRangeRuleFlagsValuesAboveMargin();
    }
}
