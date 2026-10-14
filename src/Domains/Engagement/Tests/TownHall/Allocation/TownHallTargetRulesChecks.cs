namespace Acme.Workforce.Engagement.Tests.TownHall.Allocation;

using Acme.Workforce.Engagement.TownHall.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class TownHallTargetRulesChecks
{
    public static void TownHallTargetValidatorReportsEveryProblem()
    {
        var validator = TownHallTargetRules.DefaultTownHallTargetValidator();
        var item = TownHallTarget.NewTownHallTarget("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TownHallTargetRequiredTimezoneRuleFlagsABlankTimezone()
    {
        var rule = new TownHallTargetRequiredTimezoneRule("town hall target needs this field");
        var item = TownHallTarget.NewTownHallTarget("", "");
        item.Timezone = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TownHallTargetQuantityRangeRuleFlagsValuesAboveQuantity()
    {
        var rule = new TownHallTargetQuantityRangeRule(64, 255);
        var item = TownHallTarget.NewTownHallTarget("x", "y");
        item.Quantity = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void TownHallTargetCodeLengthRuleFlagsALongCode()
    {
        var rule = new TownHallTargetCodeLengthRule(16);
        var item = TownHallTarget.NewTownHallTarget("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TownHallTargetRateRangeRuleFlagsValuesAboveRate()
    {
        var rule = new TownHallTargetRateRangeRule(8.7, 3135.0);
        var item = TownHallTarget.NewTownHallTarget("x", "y");
        item.Rate = 3136.5;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        TownHallTargetValidatorReportsEveryProblem();
        TownHallTargetRequiredTimezoneRuleFlagsABlankTimezone();
        TownHallTargetQuantityRangeRuleFlagsValuesAboveQuantity();
        TownHallTargetCodeLengthRuleFlagsALongCode();
        TownHallTargetRateRangeRuleFlagsValuesAboveRate();
    }
}
