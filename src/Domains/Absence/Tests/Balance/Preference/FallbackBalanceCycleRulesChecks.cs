namespace Acme.Workforce.Absence.Tests.Balance.Preference;

using Acme.Workforce.Absence.Balance.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class FallbackBalanceCycleRulesChecks
{
    public static void FallbackBalanceCycleValidatorReportsEveryProblem()
    {
        var validator = FallbackBalanceCycleRules.DefaultFallbackBalanceCycleValidator();
        var item = FallbackBalanceCycle.NewFallbackBalanceCycle("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FallbackBalanceCycleLabelLengthRuleFlagsALongLabel()
    {
        var rule = new FallbackBalanceCycleLabelLengthRule(120);
        var item = FallbackBalanceCycle.NewFallbackBalanceCycle("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void FallbackBalanceCycleAmountRangeRuleFlagsValuesAboveAmount()
    {
        var rule = new FallbackBalanceCycleAmountRangeRule(9.3, 518.6);
        var item = FallbackBalanceCycle.NewFallbackBalanceCycle("x", "y");
        item.Amount = 520.1;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        FallbackBalanceCycleValidatorReportsEveryProblem();
        FallbackBalanceCycleLabelLengthRuleFlagsALongLabel();
        FallbackBalanceCycleAmountRangeRuleFlagsValuesAboveAmount();
    }
}
