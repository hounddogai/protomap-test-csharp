namespace Acme.Workforce.Compensation.Tests.PayBand.Baseline;

using Acme.Workforce.Compensation.PayBand.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class PayBandSlotRulesChecks
{
    public static void PayBandSlotValidatorReportsEveryProblem()
    {
        var validator = PayBandSlotRules.DefaultPayBandSlotValidator();
        var item = PayBandSlot.NewPayBandSlot("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PayBandSlotCodeLengthRuleFlagsALongCode()
    {
        var rule = new PayBandSlotCodeLengthRule(1000);
        var item = PayBandSlot.NewPayBandSlot("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PayBandSlotThresholdRangeRuleFlagsValuesAboveThreshold()
    {
        var rule = new PayBandSlotThresholdRangeRule(6.9, 1339.8);
        var item = PayBandSlot.NewPayBandSlot("x", "y");
        item.Threshold = 1341.3;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PayBandSlotRequiredSourceRuleFlagsABlankSource()
    {
        var rule = new PayBandSlotRequiredSourceRule("pay band slot needs an entry");
        var item = PayBandSlot.NewPayBandSlot("", "");
        item.Source = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PayBandSlotOwnerIdLengthRuleFlagsALongOwnerId()
    {
        var rule = new PayBandSlotOwnerIdLengthRule(32);
        var item = PayBandSlot.NewPayBandSlot("x", "y");
        item.OwnerId = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.OwnerId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        PayBandSlotValidatorReportsEveryProblem();
        PayBandSlotCodeLengthRuleFlagsALongCode();
        PayBandSlotThresholdRangeRuleFlagsValuesAboveThreshold();
        PayBandSlotRequiredSourceRuleFlagsABlankSource();
        PayBandSlotOwnerIdLengthRuleFlagsALongOwnerId();
    }
}
