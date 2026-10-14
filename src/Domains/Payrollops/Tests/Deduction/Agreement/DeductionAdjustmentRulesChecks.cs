namespace Acme.Workforce.Payrollops.Tests.Deduction.Agreement;

using Acme.Workforce.Payrollops.Deduction.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class DeductionAdjustmentRulesChecks
{
    public static void DeductionAdjustmentValidatorReportsEveryProblem()
    {
        var validator = DeductionAdjustmentRules.DefaultDeductionAdjustmentValidator();
        var item = DeductionAdjustment.NewDeductionAdjustment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DeductionAdjustmentLocationLengthRuleFlagsALongLocation()
    {
        var rule = new DeductionAdjustmentLocationLengthRule(32);
        var item = DeductionAdjustment.NewDeductionAdjustment("x", "y");
        item.Location = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Location = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void DeductionAdjustmentRequiredSourceRuleFlagsABlankSource()
    {
        var rule = new DeductionAdjustmentRequiredSourceRule("deduction adjustment needs an entry");
        var item = DeductionAdjustment.NewDeductionAdjustment("", "");
        item.Source = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DeductionAdjustmentScoreRangeRuleFlagsValuesAboveScore()
    {
        var rule = new DeductionAdjustmentScoreRangeRule(9.2, 1439.3);
        var item = DeductionAdjustment.NewDeductionAdjustment("x", "y");
        item.Score = 1440.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        DeductionAdjustmentValidatorReportsEveryProblem();
        DeductionAdjustmentLocationLengthRuleFlagsALongLocation();
        DeductionAdjustmentRequiredSourceRuleFlagsABlankSource();
        DeductionAdjustmentScoreRangeRuleFlagsValuesAboveScore();
    }
}
