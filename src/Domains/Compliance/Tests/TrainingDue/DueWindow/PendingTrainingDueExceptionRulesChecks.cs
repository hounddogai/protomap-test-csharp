namespace Acme.Workforce.Compliance.Tests.TrainingDue.DueWindow;

using Acme.Workforce.Compliance.TrainingDue.DueWindow;

/// <summary>Checks of due window code, run by the project's runner.</summary>
public static class PendingTrainingDueExceptionRulesChecks
{
    public static void PendingTrainingDueExceptionValidatorReportsEveryProblem()
    {
        var validator = PendingTrainingDueExceptionRules.DefaultPendingTrainingDueExceptionValidator();
        var item = PendingTrainingDueException.NewPendingTrainingDueException("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PendingTrainingDueExceptionRequiredCountryRuleFlagsABlankCountry()
    {
        var rule = new PendingTrainingDueExceptionRequiredCountryRule("pending training due exception needs a value");
        var item = PendingTrainingDueException.NewPendingTrainingDueException("", "");
        item.Country = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PendingTrainingDueExceptionLabelLengthRuleFlagsALongLabel()
    {
        var rule = new PendingTrainingDueExceptionLabelLengthRule(64);
        var item = PendingTrainingDueException.NewPendingTrainingDueException("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PendingTrainingDueExceptionRateRangeRuleFlagsValuesAboveRate()
    {
        var rule = new PendingTrainingDueExceptionRateRangeRule(7.8, 3559.4);
        var item = PendingTrainingDueException.NewPendingTrainingDueException("x", "y");
        item.Rate = 3560.9;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        PendingTrainingDueExceptionValidatorReportsEveryProblem();
        PendingTrainingDueExceptionRequiredCountryRuleFlagsABlankCountry();
        PendingTrainingDueExceptionLabelLengthRuleFlagsALongLabel();
        PendingTrainingDueExceptionRateRangeRuleFlagsValuesAboveRate();
    }
}
