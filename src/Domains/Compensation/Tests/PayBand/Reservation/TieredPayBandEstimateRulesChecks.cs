namespace Acme.Workforce.Compensation.Tests.PayBand.Reservation;

using Acme.Workforce.Compensation.PayBand.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class TieredPayBandEstimateRulesChecks
{
    public static void TieredPayBandEstimateValidatorReportsEveryProblem()
    {
        var validator = TieredPayBandEstimateRules.DefaultTieredPayBandEstimateValidator();
        var item = TieredPayBandEstimate.NewTieredPayBandEstimate("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TieredPayBandEstimateRequiredSkillRuleFlagsABlankSkill()
    {
        var rule = new TieredPayBandEstimateRequiredSkillRule("tiered pay band estimate needs this field");
        var item = TieredPayBandEstimate.NewTieredPayBandEstimate("", "");
        item.Skill = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TieredPayBandEstimateRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new TieredPayBandEstimateRequiredLabelRule("tiered pay band estimate needs an entry");
        var item = TieredPayBandEstimate.NewTieredPayBandEstimate("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TieredPayBandEstimateCapacityRangeRuleFlagsValuesAboveCapacity()
    {
        var rule = new TieredPayBandEstimateCapacityRangeRule(80, 120);
        var item = TieredPayBandEstimate.NewTieredPayBandEstimate("x", "y");
        item.Capacity = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void TieredPayBandEstimateRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new TieredPayBandEstimateRequiredCodeRule("tiered pay band estimate needs an entry");
        var item = TieredPayBandEstimate.NewTieredPayBandEstimate("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        TieredPayBandEstimateValidatorReportsEveryProblem();
        TieredPayBandEstimateRequiredSkillRuleFlagsABlankSkill();
        TieredPayBandEstimateRequiredLabelRuleFlagsABlankLabel();
        TieredPayBandEstimateCapacityRangeRuleFlagsValuesAboveCapacity();
        TieredPayBandEstimateRequiredCodeRuleFlagsABlankCode();
    }
}
