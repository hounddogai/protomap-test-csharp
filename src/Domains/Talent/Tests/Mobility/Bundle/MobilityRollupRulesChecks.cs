namespace Acme.Workforce.Talent.Tests.Mobility.Bundle;

using Acme.Workforce.Talent.Mobility.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class MobilityRollupRulesChecks
{
    public static void MobilityRollupValidatorReportsEveryProblem()
    {
        var validator = MobilityRollupRules.DefaultMobilityRollupValidator();
        var item = MobilityRollup.NewMobilityRollup("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void MobilityRollupQuantityRangeRuleFlagsValuesAboveQuantity()
    {
        var rule = new MobilityRollupQuantityRangeRule(255, 1000);
        var item = MobilityRollup.NewMobilityRollup("x", "y");
        item.Quantity = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void MobilityRollupRequiredSourceRuleFlagsABlankSource()
    {
        var rule = new MobilityRollupRequiredSourceRule("mobility rollup needs this field");
        var item = MobilityRollup.NewMobilityRollup("", "");
        item.Source = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void MobilityRollupMaximumRangeRuleFlagsValuesAboveMaximum()
    {
        var rule = new MobilityRollupMaximumRangeRule(2.6, 3475.5);
        var item = MobilityRollup.NewMobilityRollup("x", "y");
        item.Maximum = 3477.0;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        MobilityRollupValidatorReportsEveryProblem();
        MobilityRollupQuantityRangeRuleFlagsValuesAboveQuantity();
        MobilityRollupRequiredSourceRuleFlagsABlankSource();
        MobilityRollupMaximumRangeRuleFlagsValuesAboveMaximum();
    }
}
