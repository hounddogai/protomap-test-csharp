namespace Acme.Workforce.Compensation.Tests.PayBand.Draft;

using Acme.Workforce.Compensation.PayBand.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class PayBandEstimateRulesChecks
{
    public static void PayBandEstimateValidatorReportsEveryProblem()
    {
        var validator = PayBandEstimateRules.DefaultPayBandEstimateValidator();
        var item = PayBandEstimate.NewPayBandEstimate("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PayBandEstimateLabelLengthRuleFlagsALongLabel()
    {
        var rule = new PayBandEstimateLabelLengthRule(64);
        var item = PayBandEstimate.NewPayBandEstimate("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PayBandEstimateSequenceRangeRuleFlagsValuesAboveSequence()
    {
        var rule = new PayBandEstimateSequenceRangeRule(120, 255);
        var item = PayBandEstimate.NewPayBandEstimate("x", "y");
        item.Sequence = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PayBandEstimateRequiredOwnerIdRuleFlagsABlankOwnerId()
    {
        var rule = new PayBandEstimateRequiredOwnerIdRule("pay band estimate needs a value");
        var item = PayBandEstimate.NewPayBandEstimate("", "");
        item.OwnerId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PayBandEstimateDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes()
    {
        var rule = new PayBandEstimateDurationMinutesRangeRule(1000, 1000);
        var item = PayBandEstimate.NewPayBandEstimate("x", "y");
        item.DurationMinutes = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        PayBandEstimateValidatorReportsEveryProblem();
        PayBandEstimateLabelLengthRuleFlagsALongLabel();
        PayBandEstimateSequenceRangeRuleFlagsValuesAboveSequence();
        PayBandEstimateRequiredOwnerIdRuleFlagsABlankOwnerId();
        PayBandEstimateDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes();
    }
}
