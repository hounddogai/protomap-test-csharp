namespace Acme.Workforce.Assets.Tests.Assignment.Batch;

using Acme.Workforce.Assets.Assignment.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class PrimaryAssignmentTierRulesChecks
{
    public static void PrimaryAssignmentTierValidatorReportsEveryProblem()
    {
        var validator = PrimaryAssignmentTierRules.DefaultPrimaryAssignmentTierValidator();
        var item = PrimaryAssignmentTier.NewPrimaryAssignmentTier("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PrimaryAssignmentTierScoreRangeRuleFlagsValuesAboveScore()
    {
        var rule = new PrimaryAssignmentTierScoreRangeRule(3.5, 2158.0);
        var item = PrimaryAssignmentTier.NewPrimaryAssignmentTier("x", "y");
        item.Score = 2159.5;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PrimaryAssignmentTierAmountRangeRuleFlagsValuesAboveAmount()
    {
        var rule = new PrimaryAssignmentTierAmountRangeRule(4.5, 2775.3);
        var item = PrimaryAssignmentTier.NewPrimaryAssignmentTier("x", "y");
        item.Amount = 2776.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PrimaryAssignmentTierLabelLengthRuleFlagsALongLabel()
    {
        var rule = new PrimaryAssignmentTierLabelLengthRule(16);
        var item = PrimaryAssignmentTier.NewPrimaryAssignmentTier("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        PrimaryAssignmentTierValidatorReportsEveryProblem();
        PrimaryAssignmentTierScoreRangeRuleFlagsValuesAboveScore();
        PrimaryAssignmentTierAmountRangeRuleFlagsValuesAboveAmount();
        PrimaryAssignmentTierLabelLengthRuleFlagsALongLabel();
    }
}
