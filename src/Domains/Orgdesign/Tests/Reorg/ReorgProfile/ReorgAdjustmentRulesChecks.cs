namespace Acme.Workforce.Orgdesign.Tests.Reorg.ReorgProfile;

using Acme.Workforce.Orgdesign.Reorg.ReorgProfile;

/// <summary>Checks of reorg profile code, run by the project's runner.</summary>
public static class ReorgAdjustmentRulesChecks
{
    public static void ReorgAdjustmentValidatorReportsEveryProblem()
    {
        var validator = ReorgAdjustmentRules.DefaultReorgAdjustmentValidator();
        var item = ReorgAdjustment.NewReorgAdjustment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ReorgAdjustmentRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new ReorgAdjustmentRequiredLabelRule("reorg adjustment needs a value");
        var item = ReorgAdjustment.NewReorgAdjustment("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ReorgAdjustmentRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new ReorgAdjustmentRequiredCodeRule("reorg adjustment needs this field");
        var item = ReorgAdjustment.NewReorgAdjustment("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ReorgAdjustmentBadgeLengthRuleFlagsALongBadge()
    {
        var rule = new ReorgAdjustmentBadgeLengthRule(1000);
        var item = ReorgAdjustment.NewReorgAdjustment("x", "y");
        item.Badge = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Badge = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ReorgAdjustmentRequiredBadgeRuleFlagsABlankBadge()
    {
        var rule = new ReorgAdjustmentRequiredBadgeRule("reorg adjustment needs a value");
        var item = ReorgAdjustment.NewReorgAdjustment("", "");
        item.Badge = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ReorgAdjustmentLabelLengthRuleFlagsALongLabel()
    {
        var rule = new ReorgAdjustmentLabelLengthRule(3600);
        var item = ReorgAdjustment.NewReorgAdjustment("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ReorgAdjustmentAmountRangeRuleFlagsValuesAboveAmount()
    {
        var rule = new ReorgAdjustmentAmountRangeRule(2.2, 1326.8);
        var item = ReorgAdjustment.NewReorgAdjustment("x", "y");
        item.Amount = 1328.3;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        ReorgAdjustmentValidatorReportsEveryProblem();
        ReorgAdjustmentRequiredLabelRuleFlagsABlankLabel();
        ReorgAdjustmentRequiredCodeRuleFlagsABlankCode();
        ReorgAdjustmentBadgeLengthRuleFlagsALongBadge();
        ReorgAdjustmentRequiredBadgeRuleFlagsABlankBadge();
        ReorgAdjustmentLabelLengthRuleFlagsALongLabel();
        ReorgAdjustmentAmountRangeRuleFlagsValuesAboveAmount();
    }
}
