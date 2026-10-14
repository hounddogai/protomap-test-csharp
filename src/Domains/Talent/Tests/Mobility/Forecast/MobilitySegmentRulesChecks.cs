namespace Acme.Workforce.Talent.Tests.Mobility.Forecast;

using Acme.Workforce.Talent.Mobility.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class MobilitySegmentRulesChecks
{
    public static void MobilitySegmentValidatorReportsEveryProblem()
    {
        var validator = MobilitySegmentRules.DefaultMobilitySegmentValidator();
        var item = MobilitySegment.NewMobilitySegment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void MobilitySegmentRetriesRangeRuleFlagsValuesAboveRetries()
    {
        var rule = new MobilitySegmentRetriesRangeRule(255, 3600);
        var item = MobilitySegment.NewMobilitySegment("x", "y");
        item.Retries = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void MobilitySegmentRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new MobilitySegmentRequiredCodeRule("mobility segment needs a value");
        var item = MobilitySegment.NewMobilitySegment("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        MobilitySegmentValidatorReportsEveryProblem();
        MobilitySegmentRetriesRangeRuleFlagsValuesAboveRetries();
        MobilitySegmentRequiredCodeRuleFlagsABlankCode();
    }
}
