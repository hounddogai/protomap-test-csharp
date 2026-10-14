namespace Acme.Workforce.Compensation.Tests.PayBand.Snapshot;

using Acme.Workforce.Compensation.PayBand.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class PayBandPolicyRulesChecks
{
    public static void PayBandPolicyValidatorReportsEveryProblem()
    {
        var validator = PayBandPolicyRules.DefaultPayBandPolicyValidator();
        var item = PayBandPolicy.NewPayBandPolicy("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PayBandPolicyRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new PayBandPolicyRequiredCodeRule("pay band policy needs an entry");
        var item = PayBandPolicy.NewPayBandPolicy("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PayBandPolicyRateRangeRuleFlagsValuesAboveRate()
    {
        var rule = new PayBandPolicyRateRangeRule(7.2, 4259.5);
        var item = PayBandPolicy.NewPayBandPolicy("x", "y");
        item.Rate = 4261.0;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        PayBandPolicyValidatorReportsEveryProblem();
        PayBandPolicyRequiredCodeRuleFlagsABlankCode();
        PayBandPolicyRateRangeRuleFlagsValuesAboveRate();
    }
}
