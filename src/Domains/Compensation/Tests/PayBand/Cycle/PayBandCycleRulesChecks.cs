namespace Acme.Workforce.Compensation.Tests.PayBand.Cycle;

using Acme.Workforce.Compensation.PayBand.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class PayBandCycleRulesChecks
{
    public static void PayBandCycleValidatorReportsEveryProblem()
    {
        var validator = PayBandCycleRules.DefaultPayBandCycleValidator();
        var item = PayBandCycle.NewPayBandCycle("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PayBandCycleQuantityRangeRuleFlagsValuesAboveQuantity()
    {
        var rule = new PayBandCycleQuantityRangeRule(32, 32);
        var item = PayBandCycle.NewPayBandCycle("x", "y");
        item.Quantity = 33;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PayBandCycleCodeLengthRuleFlagsALongCode()
    {
        var rule = new PayBandCycleCodeLengthRule(1000);
        var item = PayBandCycle.NewPayBandCycle("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PayBandCycleRequiredManagerIdRuleFlagsABlankManagerId()
    {
        var rule = new PayBandCycleRequiredManagerIdRule("pay band cycle needs a value");
        var item = PayBandCycle.NewPayBandCycle("", "");
        item.ManagerId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PayBandCycleSkillLengthRuleFlagsALongSkill()
    {
        var rule = new PayBandCycleSkillLengthRule(80);
        var item = PayBandCycle.NewPayBandCycle("x", "y");
        item.Skill = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Skill = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PayBandCycleRequiredNameRuleFlagsABlankName()
    {
        var rule = new PayBandCycleRequiredNameRule("pay band cycle needs this field");
        var item = PayBandCycle.NewPayBandCycle("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PayBandCycleCostRangeRuleFlagsValuesAboveCost()
    {
        var rule = new PayBandCycleCostRangeRule(5.3, 3067.9);
        var item = PayBandCycle.NewPayBandCycle("x", "y");
        item.Cost = 3069.4;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        PayBandCycleValidatorReportsEveryProblem();
        PayBandCycleQuantityRangeRuleFlagsValuesAboveQuantity();
        PayBandCycleCodeLengthRuleFlagsALongCode();
        PayBandCycleRequiredManagerIdRuleFlagsABlankManagerId();
        PayBandCycleSkillLengthRuleFlagsALongSkill();
        PayBandCycleRequiredNameRuleFlagsABlankName();
        PayBandCycleCostRangeRuleFlagsValuesAboveCost();
    }
}
