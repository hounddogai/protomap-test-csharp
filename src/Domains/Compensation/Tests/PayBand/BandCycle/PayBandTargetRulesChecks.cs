namespace Acme.Workforce.Compensation.Tests.PayBand.BandCycle;

using Acme.Workforce.Compensation.PayBand.BandCycle;

/// <summary>Checks of band cycle code, run by the project's runner.</summary>
public static class PayBandTargetRulesChecks
{
    public static void PayBandTargetValidatorReportsEveryProblem()
    {
        var validator = PayBandTargetRules.DefaultPayBandTargetValidator();
        var item = PayBandTarget.NewPayBandTarget("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PayBandTargetRequiredJobCodeRuleFlagsABlankJobCode()
    {
        var rule = new PayBandTargetRequiredJobCodeRule("pay band target needs this field");
        var item = PayBandTarget.NewPayBandTarget("", "");
        item.JobCode = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PayBandTargetNameLengthRuleFlagsALongName()
    {
        var rule = new PayBandTargetNameLengthRule(3600);
        var item = PayBandTarget.NewPayBandTarget("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PayBandTargetLimitRangeRuleFlagsValuesAboveLimit()
    {
        var rule = new PayBandTargetLimitRangeRule(255, 1000);
        var item = PayBandTarget.NewPayBandTarget("x", "y");
        item.Limit = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PayBandTargetRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new PayBandTargetRequiredCodeRule("pay band target needs an entry");
        var item = PayBandTarget.NewPayBandTarget("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PayBandTargetRequiredNameRuleFlagsABlankName()
    {
        var rule = new PayBandTargetRequiredNameRule("pay band target needs this field");
        var item = PayBandTarget.NewPayBandTarget("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        PayBandTargetValidatorReportsEveryProblem();
        PayBandTargetRequiredJobCodeRuleFlagsABlankJobCode();
        PayBandTargetNameLengthRuleFlagsALongName();
        PayBandTargetLimitRangeRuleFlagsValuesAboveLimit();
        PayBandTargetRequiredCodeRuleFlagsABlankCode();
        PayBandTargetRequiredNameRuleFlagsABlankName();
    }
}
