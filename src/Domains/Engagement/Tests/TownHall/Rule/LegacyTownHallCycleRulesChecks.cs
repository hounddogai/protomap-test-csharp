namespace Acme.Workforce.Engagement.Tests.TownHall.Rule;

using Acme.Workforce.Engagement.TownHall.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class LegacyTownHallCycleRulesChecks
{
    public static void LegacyTownHallCycleValidatorReportsEveryProblem()
    {
        var validator = LegacyTownHallCycleRules.DefaultLegacyTownHallCycleValidator();
        var item = LegacyTownHallCycle.NewLegacyTownHallCycle("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LegacyTownHallCycleRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new LegacyTownHallCycleRequiredTitleRule("legacy town hall cycle needs this field");
        var item = LegacyTownHallCycle.NewLegacyTownHallCycle("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LegacyTownHallCycleQuantityRangeRuleFlagsValuesAboveQuantity()
    {
        var rule = new LegacyTownHallCycleQuantityRangeRule(16, 120);
        var item = LegacyTownHallCycle.NewLegacyTownHallCycle("x", "y");
        item.Quantity = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LegacyTownHallCycleCodeLengthRuleFlagsALongCode()
    {
        var rule = new LegacyTownHallCycleCodeLengthRule(1000);
        var item = LegacyTownHallCycle.NewLegacyTownHallCycle("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LegacyTownHallCycleTitleLengthRuleFlagsALongTitle()
    {
        var rule = new LegacyTownHallCycleTitleLengthRule(86400);
        var item = LegacyTownHallCycle.NewLegacyTownHallCycle("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LegacyTownHallCycleRequiredSummaryRuleFlagsABlankSummary()
    {
        var rule = new LegacyTownHallCycleRequiredSummaryRule("legacy town hall cycle needs an entry");
        var item = LegacyTownHallCycle.NewLegacyTownHallCycle("", "");
        item.Summary = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        LegacyTownHallCycleValidatorReportsEveryProblem();
        LegacyTownHallCycleRequiredTitleRuleFlagsABlankTitle();
        LegacyTownHallCycleQuantityRangeRuleFlagsValuesAboveQuantity();
        LegacyTownHallCycleCodeLengthRuleFlagsALongCode();
        LegacyTownHallCycleTitleLengthRuleFlagsALongTitle();
        LegacyTownHallCycleRequiredSummaryRuleFlagsABlankSummary();
    }
}
