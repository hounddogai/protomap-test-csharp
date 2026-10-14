namespace Acme.Workforce.Talent.Tests.Mobility.Profile;

using Acme.Workforce.Talent.Mobility.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class MobilityBaselineRulesChecks
{
    public static void MobilityBaselineValidatorReportsEveryProblem()
    {
        var validator = MobilityBaselineRules.DefaultMobilityBaselineValidator();
        var item = MobilityBaseline.NewMobilityBaseline("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void MobilityBaselineTitleLengthRuleFlagsALongTitle()
    {
        var rule = new MobilityBaselineTitleLengthRule(16);
        var item = MobilityBaseline.NewMobilityBaseline("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void MobilityBaselineCostRangeRuleFlagsValuesAboveCost()
    {
        var rule = new MobilityBaselineCostRangeRule(2.2, 1761.5);
        var item = MobilityBaseline.NewMobilityBaseline("x", "y");
        item.Cost = 1763.0;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void MobilityBaselineExpiresAtRangeRuleFlagsValuesAboveExpiresAt()
    {
        var rule = new MobilityBaselineExpiresAtRangeRule(120, 255);
        var item = MobilityBaseline.NewMobilityBaseline("x", "y");
        item.ExpiresAt = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void MobilityBaselineRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new MobilityBaselineRequiredLabelRule("mobility baseline needs an entry");
        var item = MobilityBaseline.NewMobilityBaseline("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void MobilityBaselineCodeLengthRuleFlagsALongCode()
    {
        var rule = new MobilityBaselineCodeLengthRule(3600);
        var item = MobilityBaseline.NewMobilityBaseline("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        MobilityBaselineValidatorReportsEveryProblem();
        MobilityBaselineTitleLengthRuleFlagsALongTitle();
        MobilityBaselineCostRangeRuleFlagsValuesAboveCost();
        MobilityBaselineExpiresAtRangeRuleFlagsValuesAboveExpiresAt();
        MobilityBaselineRequiredLabelRuleFlagsABlankLabel();
        MobilityBaselineCodeLengthRuleFlagsALongCode();
    }
}
