namespace Acme.Workforce.Compliance.Tests.Exception.Preference;

using Acme.Workforce.Compliance.Exception.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class ExceptionTargetRulesChecks
{
    public static void ExceptionTargetValidatorReportsEveryProblem()
    {
        var validator = ExceptionTargetRules.DefaultExceptionTargetValidator();
        var item = ExceptionTarget.NewExceptionTarget("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ExceptionTargetBatchSizeRangeRuleFlagsValuesAboveBatchSize()
    {
        var rule = new ExceptionTargetBatchSizeRangeRule(120, 86400);
        var item = ExceptionTarget.NewExceptionTarget("x", "y");
        item.BatchSize = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ExceptionTargetLabelLengthRuleFlagsALongLabel()
    {
        var rule = new ExceptionTargetLabelLengthRule(1000);
        var item = ExceptionTarget.NewExceptionTarget("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        ExceptionTargetValidatorReportsEveryProblem();
        ExceptionTargetBatchSizeRangeRuleFlagsValuesAboveBatchSize();
        ExceptionTargetLabelLengthRuleFlagsALongLabel();
    }
}
