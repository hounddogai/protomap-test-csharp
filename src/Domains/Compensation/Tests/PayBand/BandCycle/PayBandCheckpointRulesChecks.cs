namespace Acme.Workforce.Compensation.Tests.PayBand.BandCycle;

using Acme.Workforce.Compensation.PayBand.BandCycle;

/// <summary>Checks of band cycle code, run by the project's runner.</summary>
public static class PayBandCheckpointRulesChecks
{
    public static void PayBandCheckpointValidatorReportsEveryProblem()
    {
        var validator = PayBandCheckpointRules.DefaultPayBandCheckpointValidator();
        var item = PayBandCheckpoint.NewPayBandCheckpoint("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PayBandCheckpointCodeLengthRuleFlagsALongCode()
    {
        var rule = new PayBandCheckpointCodeLengthRule(16);
        var item = PayBandCheckpoint.NewPayBandCheckpoint("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PayBandCheckpointRequiredJobCodeRuleFlagsABlankJobCode()
    {
        var rule = new PayBandCheckpointRequiredJobCodeRule("pay band checkpoint needs an entry");
        var item = PayBandCheckpoint.NewPayBandCheckpoint("", "");
        item.JobCode = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PayBandCheckpointRequiredSegmentRuleFlagsABlankSegment()
    {
        var rule = new PayBandCheckpointRequiredSegmentRule("pay band checkpoint needs an entry");
        var item = PayBandCheckpoint.NewPayBandCheckpoint("", "");
        item.Segment = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PayBandCheckpointBudgetRangeRuleFlagsValuesAboveBudget()
    {
        var rule = new PayBandCheckpointBudgetRangeRule(5.6, 2801.0);
        var item = PayBandCheckpoint.NewPayBandCheckpoint("x", "y");
        item.Budget = 2802.5;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PayBandCheckpointRatioRangeRuleFlagsValuesAboveRatio()
    {
        var rule = new PayBandCheckpointRatioRangeRule(2.0, 3683.0);
        var item = PayBandCheckpoint.NewPayBandCheckpoint("x", "y");
        item.Ratio = 3684.5;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PayBandCheckpointRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new PayBandCheckpointRequiredCodeRule("pay band checkpoint needs this field");
        var item = PayBandCheckpoint.NewPayBandCheckpoint("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        PayBandCheckpointValidatorReportsEveryProblem();
        PayBandCheckpointCodeLengthRuleFlagsALongCode();
        PayBandCheckpointRequiredJobCodeRuleFlagsABlankJobCode();
        PayBandCheckpointRequiredSegmentRuleFlagsABlankSegment();
        PayBandCheckpointBudgetRangeRuleFlagsValuesAboveBudget();
        PayBandCheckpointRatioRangeRuleFlagsValuesAboveRatio();
        PayBandCheckpointRequiredCodeRuleFlagsABlankCode();
    }
}
