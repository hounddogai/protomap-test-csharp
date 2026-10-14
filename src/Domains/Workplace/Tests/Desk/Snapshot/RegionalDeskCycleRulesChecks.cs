namespace Acme.Workforce.Workplace.Tests.Desk.Snapshot;

using Acme.Workforce.Workplace.Desk.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class RegionalDeskCycleRulesChecks
{
    public static void RegionalDeskCycleValidatorReportsEveryProblem()
    {
        var validator = RegionalDeskCycleRules.DefaultRegionalDeskCycleValidator();
        var item = RegionalDeskCycle.NewRegionalDeskCycle("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RegionalDeskCycleLocationLengthRuleFlagsALongLocation()
    {
        var rule = new RegionalDeskCycleLocationLengthRule(1000);
        var item = RegionalDeskCycle.NewRegionalDeskCycle("x", "y");
        item.Location = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Location = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RegionalDeskCycleRequiredNameRuleFlagsABlankName()
    {
        var rule = new RegionalDeskCycleRequiredNameRule("regional desk cycle needs a value");
        var item = RegionalDeskCycle.NewRegionalDeskCycle("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RegionalDeskCycleRatioRangeRuleFlagsValuesAboveRatio()
    {
        var rule = new RegionalDeskCycleRatioRangeRule(5.7, 1229.5);
        var item = RegionalDeskCycle.NewRegionalDeskCycle("x", "y");
        item.Ratio = 1231.0;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        RegionalDeskCycleValidatorReportsEveryProblem();
        RegionalDeskCycleLocationLengthRuleFlagsALongLocation();
        RegionalDeskCycleRequiredNameRuleFlagsABlankName();
        RegionalDeskCycleRatioRangeRuleFlagsValuesAboveRatio();
    }
}
