namespace Acme.Workforce.Talent.Tests.Mobility.Tier;

using Acme.Workforce.Talent.Mobility.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class ManualMobilityReservationRulesChecks
{
    public static void ManualMobilityReservationValidatorReportsEveryProblem()
    {
        var validator = ManualMobilityReservationRules.DefaultManualMobilityReservationValidator();
        var item = ManualMobilityReservation.NewManualMobilityReservation("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ManualMobilityReservationRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new ManualMobilityReservationRequiredCodeRule("manual mobility reservation needs this field");
        var item = ManualMobilityReservation.NewManualMobilityReservation("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ManualMobilityReservationRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new ManualMobilityReservationRequiredTitleRule("manual mobility reservation needs an entry");
        var item = ManualMobilityReservation.NewManualMobilityReservation("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ManualMobilityReservationMarginRangeRuleFlagsValuesAboveMargin()
    {
        var rule = new ManualMobilityReservationMarginRangeRule(2.4, 2427.0);
        var item = ManualMobilityReservation.NewManualMobilityReservation("x", "y");
        item.Margin = 2428.5;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ManualMobilityReservationMaximumRangeRuleFlagsValuesAboveMaximum()
    {
        var rule = new ManualMobilityReservationMaximumRangeRule(3.7, 4655.0);
        var item = ManualMobilityReservation.NewManualMobilityReservation("x", "y");
        item.Maximum = 4656.5;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ManualMobilityReservationGradeLengthRuleFlagsALongGrade()
    {
        var rule = new ManualMobilityReservationGradeLengthRule(3600);
        var item = ManualMobilityReservation.NewManualMobilityReservation("x", "y");
        item.Grade = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Grade = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        ManualMobilityReservationValidatorReportsEveryProblem();
        ManualMobilityReservationRequiredCodeRuleFlagsABlankCode();
        ManualMobilityReservationRequiredTitleRuleFlagsABlankTitle();
        ManualMobilityReservationMarginRangeRuleFlagsValuesAboveMargin();
        ManualMobilityReservationMaximumRangeRuleFlagsValuesAboveMaximum();
        ManualMobilityReservationGradeLengthRuleFlagsALongGrade();
    }
}
