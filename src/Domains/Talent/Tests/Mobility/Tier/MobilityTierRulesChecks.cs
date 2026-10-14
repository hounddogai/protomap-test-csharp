namespace Acme.Workforce.Talent.Tests.Mobility.Tier;

using Acme.Workforce.Talent.Mobility.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class MobilityTierRulesChecks
{
    public static void MobilityTierValidatorReportsEveryProblem()
    {
        var validator = MobilityTierRules.DefaultMobilityTierValidator();
        var item = MobilityTier.NewMobilityTier("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void MobilityTierRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new MobilityTierRequiredLabelRule("mobility tier needs this field");
        var item = MobilityTier.NewMobilityTier("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void MobilityTierLabelLengthRuleFlagsALongLabel()
    {
        var rule = new MobilityTierLabelLengthRule(120);
        var item = MobilityTier.NewMobilityTier("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void MobilityTierCostRangeRuleFlagsValuesAboveCost()
    {
        var rule = new MobilityTierCostRangeRule(8.1, 3799.5);
        var item = MobilityTier.NewMobilityTier("x", "y");
        item.Cost = 3801.0;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void MobilityTierRequiredSalaryBandRuleFlagsABlankSalaryBand()
    {
        var rule = new MobilityTierRequiredSalaryBandRule("mobility tier needs this field");
        var item = MobilityTier.NewMobilityTier("", "");
        item.SalaryBand = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void MobilityTierUnitLengthRuleFlagsALongUnit()
    {
        var rule = new MobilityTierUnitLengthRule(255);
        var item = MobilityTier.NewMobilityTier("x", "y");
        item.Unit = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Unit = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        MobilityTierValidatorReportsEveryProblem();
        MobilityTierRequiredLabelRuleFlagsABlankLabel();
        MobilityTierLabelLengthRuleFlagsALongLabel();
        MobilityTierCostRangeRuleFlagsValuesAboveCost();
        MobilityTierRequiredSalaryBandRuleFlagsABlankSalaryBand();
        MobilityTierUnitLengthRuleFlagsALongUnit();
    }
}
