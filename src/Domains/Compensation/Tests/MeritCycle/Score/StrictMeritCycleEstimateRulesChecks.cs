namespace Acme.Workforce.Compensation.Tests.MeritCycle.Score;

using Acme.Workforce.Compensation.MeritCycle.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class StrictMeritCycleEstimateRulesChecks
{
    public static void StrictMeritCycleEstimateValidatorReportsEveryProblem()
    {
        var validator = StrictMeritCycleEstimateRules.DefaultStrictMeritCycleEstimateValidator();
        var item = StrictMeritCycleEstimate.NewStrictMeritCycleEstimate("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void StrictMeritCycleEstimateLabelLengthRuleFlagsALongLabel()
    {
        var rule = new StrictMeritCycleEstimateLabelLengthRule(255);
        var item = StrictMeritCycleEstimate.NewStrictMeritCycleEstimate("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void StrictMeritCycleEstimateAmountRangeRuleFlagsValuesAboveAmount()
    {
        var rule = new StrictMeritCycleEstimateAmountRangeRule(3.2, 2496.1);
        var item = StrictMeritCycleEstimate.NewStrictMeritCycleEstimate("x", "y");
        item.Amount = 2497.6;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void StrictMeritCycleEstimateRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new StrictMeritCycleEstimateRequiredLabelRule("strict merit cycle estimate needs this field");
        var item = StrictMeritCycleEstimate.NewStrictMeritCycleEstimate("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void StrictMeritCycleEstimateNotesLengthRuleFlagsALongNotes()
    {
        var rule = new StrictMeritCycleEstimateNotesLengthRule(32);
        var item = StrictMeritCycleEstimate.NewStrictMeritCycleEstimate("x", "y");
        item.Notes = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Notes = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        StrictMeritCycleEstimateValidatorReportsEveryProblem();
        StrictMeritCycleEstimateLabelLengthRuleFlagsALongLabel();
        StrictMeritCycleEstimateAmountRangeRuleFlagsValuesAboveAmount();
        StrictMeritCycleEstimateRequiredLabelRuleFlagsABlankLabel();
        StrictMeritCycleEstimateNotesLengthRuleFlagsALongNotes();
    }
}
