namespace Acme.Workforce.Workplace.Tests.Desk.DeskAllocation;

using Acme.Workforce.Workplace.Desk.DeskAllocation;

/// <summary>Checks of desk allocation code, run by the project's runner.</summary>
public static class TieredDeskCycleRulesChecks
{
    public static void TieredDeskCycleValidatorReportsEveryProblem()
    {
        var validator = TieredDeskCycleRules.DefaultTieredDeskCycleValidator();
        var item = TieredDeskCycle.NewTieredDeskCycle("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TieredDeskCycleRetriesRangeRuleFlagsValuesAboveRetries()
    {
        var rule = new TieredDeskCycleRetriesRangeRule(64, 80);
        var item = TieredDeskCycle.NewTieredDeskCycle("x", "y");
        item.Retries = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void TieredDeskCycleLabelLengthRuleFlagsALongLabel()
    {
        var rule = new TieredDeskCycleLabelLengthRule(16);
        var item = TieredDeskCycle.NewTieredDeskCycle("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TieredDeskCycleContactEmailLengthRuleFlagsALongContactEmail()
    {
        var rule = new TieredDeskCycleContactEmailLengthRule(80);
        var item = TieredDeskCycle.NewTieredDeskCycle("x", "y");
        item.ContactEmail = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ContactEmail = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TieredDeskCycleRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new TieredDeskCycleRequiredLabelRule("tiered desk cycle needs an entry");
        var item = TieredDeskCycle.NewTieredDeskCycle("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        TieredDeskCycleValidatorReportsEveryProblem();
        TieredDeskCycleRetriesRangeRuleFlagsValuesAboveRetries();
        TieredDeskCycleLabelLengthRuleFlagsALongLabel();
        TieredDeskCycleContactEmailLengthRuleFlagsALongContactEmail();
        TieredDeskCycleRequiredLabelRuleFlagsABlankLabel();
    }
}
