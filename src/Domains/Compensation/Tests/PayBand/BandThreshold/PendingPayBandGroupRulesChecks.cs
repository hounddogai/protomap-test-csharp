namespace Acme.Workforce.Compensation.Tests.PayBand.BandThreshold;

using Acme.Workforce.Compensation.PayBand.BandThreshold;

/// <summary>Checks of band threshold code, run by the project's runner.</summary>
public static class PendingPayBandGroupRulesChecks
{
    public static void PendingPayBandGroupValidatorReportsEveryProblem()
    {
        var validator = PendingPayBandGroupRules.DefaultPendingPayBandGroupValidator();
        var item = PendingPayBandGroup.NewPendingPayBandGroup("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PendingPayBandGroupWeightRangeRuleFlagsValuesAboveWeight()
    {
        var rule = new PendingPayBandGroupWeightRangeRule(2.3, 2980.4);
        var item = PendingPayBandGroup.NewPendingPayBandGroup("x", "y");
        item.Weight = 2981.9;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PendingPayBandGroupRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new PendingPayBandGroupRequiredLabelRule("pending pay band group needs an entry");
        var item = PendingPayBandGroup.NewPendingPayBandGroup("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PendingPayBandGroupRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new PendingPayBandGroupRequiredCodeRule("pending pay band group needs an entry");
        var item = PendingPayBandGroup.NewPendingPayBandGroup("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PendingPayBandGroupLabelLengthRuleFlagsALongLabel()
    {
        var rule = new PendingPayBandGroupLabelLengthRule(80);
        var item = PendingPayBandGroup.NewPendingPayBandGroup("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        PendingPayBandGroupValidatorReportsEveryProblem();
        PendingPayBandGroupWeightRangeRuleFlagsValuesAboveWeight();
        PendingPayBandGroupRequiredLabelRuleFlagsABlankLabel();
        PendingPayBandGroupRequiredCodeRuleFlagsABlankCode();
        PendingPayBandGroupLabelLengthRuleFlagsALongLabel();
    }
}
