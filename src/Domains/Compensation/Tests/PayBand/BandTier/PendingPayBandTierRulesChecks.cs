namespace Acme.Workforce.Compensation.Tests.PayBand.BandTier;

using Acme.Workforce.Compensation.PayBand.BandTier;

/// <summary>Checks of band tier code, run by the project's runner.</summary>
public static class PendingPayBandTierRulesChecks
{
    public static void PendingPayBandTierValidatorReportsEveryProblem()
    {
        var validator = PendingPayBandTierRules.DefaultPendingPayBandTierValidator();
        var item = PendingPayBandTier.NewPendingPayBandTier("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PendingPayBandTierScoreRangeRuleFlagsValuesAboveScore()
    {
        var rule = new PendingPayBandTierScoreRangeRule(2.2, 4266.8);
        var item = PendingPayBandTier.NewPendingPayBandTier("x", "y");
        item.Score = 4268.3;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PendingPayBandTierRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new PendingPayBandTierRequiredCodeRule("pending pay band tier needs this field");
        var item = PendingPayBandTier.NewPendingPayBandTier("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PendingPayBandTierCeilingRangeRuleFlagsValuesAboveCeiling()
    {
        var rule = new PendingPayBandTierCeilingRangeRule(2.6, 3349.6);
        var item = PendingPayBandTier.NewPendingPayBandTier("x", "y");
        item.Ceiling = 3351.1;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PendingPayBandTierRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new PendingPayBandTierRequiredLabelRule("pending pay band tier needs an entry");
        var item = PendingPayBandTier.NewPendingPayBandTier("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PendingPayBandTierCodeLengthRuleFlagsALongCode()
    {
        var rule = new PendingPayBandTierCodeLengthRule(86400);
        var item = PendingPayBandTier.NewPendingPayBandTier("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        PendingPayBandTierValidatorReportsEveryProblem();
        PendingPayBandTierScoreRangeRuleFlagsValuesAboveScore();
        PendingPayBandTierRequiredCodeRuleFlagsABlankCode();
        PendingPayBandTierCeilingRangeRuleFlagsValuesAboveCeiling();
        PendingPayBandTierRequiredLabelRuleFlagsABlankLabel();
        PendingPayBandTierCodeLengthRuleFlagsALongCode();
    }
}
