namespace Acme.Workforce.Talent.Tests.Pool.Adjustment;

using Acme.Workforce.Talent.Pool.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class ManualPoolTierRulesChecks
{
    public static void ManualPoolTierValidatorReportsEveryProblem()
    {
        var validator = ManualPoolTierRules.DefaultManualPoolTierValidator();
        var item = ManualPoolTier.NewManualPoolTier("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ManualPoolTierNameLengthRuleFlagsALongName()
    {
        var rule = new ManualPoolTierNameLengthRule(86400);
        var item = ManualPoolTier.NewManualPoolTier("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ManualPoolTierRequiredNameRuleFlagsABlankName()
    {
        var rule = new ManualPoolTierRequiredNameRule("manual pool tier needs this field");
        var item = ManualPoolTier.NewManualPoolTier("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ManualPoolTierMaximumRangeRuleFlagsValuesAboveMaximum()
    {
        var rule = new ManualPoolTierMaximumRangeRule(1.9, 2470.4);
        var item = ManualPoolTier.NewManualPoolTier("x", "y");
        item.Maximum = 2471.9;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        ManualPoolTierValidatorReportsEveryProblem();
        ManualPoolTierNameLengthRuleFlagsALongName();
        ManualPoolTierRequiredNameRuleFlagsABlankName();
        ManualPoolTierMaximumRangeRuleFlagsValuesAboveMaximum();
    }
}
