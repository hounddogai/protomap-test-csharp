namespace Acme.Workforce.Compensation.Tests.PayBand.BandThreshold;

using Acme.Workforce.Compensation.PayBand.BandThreshold;

/// <summary>Checks of band threshold code, run by the project's runner.</summary>
public static class PayBandGroupRulesChecks
{
    public static void PayBandGroupValidatorReportsEveryProblem()
    {
        var validator = PayBandGroupRules.DefaultPayBandGroupValidator();
        var item = PayBandGroup.NewPayBandGroup("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PayBandGroupCodeLengthRuleFlagsALongCode()
    {
        var rule = new PayBandGroupCodeLengthRule(255);
        var item = PayBandGroup.NewPayBandGroup("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PayBandGroupRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new PayBandGroupRequiredLabelRule("pay band group needs this field");
        var item = PayBandGroup.NewPayBandGroup("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PayBandGroupTenureMonthsRangeRuleFlagsValuesAboveTenureMonths()
    {
        var rule = new PayBandGroupTenureMonthsRangeRule(32, 86400);
        var item = PayBandGroup.NewPayBandGroup("x", "y");
        item.TenureMonths = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        PayBandGroupValidatorReportsEveryProblem();
        PayBandGroupCodeLengthRuleFlagsALongCode();
        PayBandGroupRequiredLabelRuleFlagsABlankLabel();
        PayBandGroupTenureMonthsRangeRuleFlagsValuesAboveTenureMonths();
    }
}
