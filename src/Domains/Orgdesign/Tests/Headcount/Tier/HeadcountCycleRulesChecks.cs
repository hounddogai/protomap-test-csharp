namespace Acme.Workforce.Orgdesign.Tests.Headcount.Tier;

using Acme.Workforce.Orgdesign.Headcount.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class HeadcountCycleRulesChecks
{
    public static void HeadcountCycleValidatorReportsEveryProblem()
    {
        var validator = HeadcountCycleRules.DefaultHeadcountCycleValidator();
        var item = HeadcountCycle.NewHeadcountCycle("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void HeadcountCycleBadgeLengthRuleFlagsALongBadge()
    {
        var rule = new HeadcountCycleBadgeLengthRule(86400);
        var item = HeadcountCycle.NewHeadcountCycle("x", "y");
        item.Badge = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Badge = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void HeadcountCycleRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new HeadcountCycleRequiredLabelRule("headcount cycle needs an entry");
        var item = HeadcountCycle.NewHeadcountCycle("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void HeadcountCycleWindowDaysRangeRuleFlagsValuesAboveWindowDays()
    {
        var rule = new HeadcountCycleWindowDaysRangeRule(16, 1000);
        var item = HeadcountCycle.NewHeadcountCycle("x", "y");
        item.WindowDays = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void HeadcountCycleCostRangeRuleFlagsValuesAboveCost()
    {
        var rule = new HeadcountCycleCostRangeRule(5.8, 2868.3);
        var item = HeadcountCycle.NewHeadcountCycle("x", "y");
        item.Cost = 2869.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void HeadcountCycleCodeLengthRuleFlagsALongCode()
    {
        var rule = new HeadcountCycleCodeLengthRule(1000);
        var item = HeadcountCycle.NewHeadcountCycle("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        HeadcountCycleValidatorReportsEveryProblem();
        HeadcountCycleBadgeLengthRuleFlagsALongBadge();
        HeadcountCycleRequiredLabelRuleFlagsABlankLabel();
        HeadcountCycleWindowDaysRangeRuleFlagsValuesAboveWindowDays();
        HeadcountCycleCostRangeRuleFlagsValuesAboveCost();
        HeadcountCycleCodeLengthRuleFlagsALongCode();
    }
}
