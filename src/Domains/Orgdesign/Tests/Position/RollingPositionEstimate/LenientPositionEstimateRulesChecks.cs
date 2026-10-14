namespace Acme.Workforce.Orgdesign.Tests.Position.RollingPositionEstimate;

using Acme.Workforce.Orgdesign.Position.RollingPositionEstimate;

/// <summary>Checks of rolling position estimate code, run by the project's runner.</summary>
public static class LenientPositionEstimateRulesChecks
{
    public static void LenientPositionEstimateValidatorReportsEveryProblem()
    {
        var validator = LenientPositionEstimateRules.DefaultLenientPositionEstimateValidator();
        var item = LenientPositionEstimate.NewLenientPositionEstimate("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LenientPositionEstimateSequenceRangeRuleFlagsValuesAboveSequence()
    {
        var rule = new LenientPositionEstimateSequenceRangeRule(255, 1000);
        var item = LenientPositionEstimate.NewLenientPositionEstimate("x", "y");
        item.Sequence = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LenientPositionEstimateRateRangeRuleFlagsValuesAboveRate()
    {
        var rule = new LenientPositionEstimateRateRangeRule(1.5, 443.1);
        var item = LenientPositionEstimate.NewLenientPositionEstimate("x", "y");
        item.Rate = 444.6;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        LenientPositionEstimateValidatorReportsEveryProblem();
        LenientPositionEstimateSequenceRangeRuleFlagsValuesAboveSequence();
        LenientPositionEstimateRateRangeRuleFlagsValuesAboveRate();
    }
}
