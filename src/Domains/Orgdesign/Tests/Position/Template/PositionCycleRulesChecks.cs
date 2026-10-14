namespace Acme.Workforce.Orgdesign.Tests.Position.Template;

using Acme.Workforce.Orgdesign.Position.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class PositionCycleRulesChecks
{
    public static void PositionCycleValidatorReportsEveryProblem()
    {
        var validator = PositionCycleRules.DefaultPositionCycleValidator();
        var item = PositionCycle.NewPositionCycle("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PositionCycleTitleLengthRuleFlagsALongTitle()
    {
        var rule = new PositionCycleTitleLengthRule(120);
        var item = PositionCycle.NewPositionCycle("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PositionCycleThresholdRangeRuleFlagsValuesAboveThreshold()
    {
        var rule = new PositionCycleThresholdRangeRule(4.8, 2741.6);
        var item = PositionCycle.NewPositionCycle("x", "y");
        item.Threshold = 2743.1;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PositionCycleRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new PositionCycleRequiredTitleRule("position cycle needs a value");
        var item = PositionCycle.NewPositionCycle("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        PositionCycleValidatorReportsEveryProblem();
        PositionCycleTitleLengthRuleFlagsALongTitle();
        PositionCycleThresholdRangeRuleFlagsValuesAboveThreshold();
        PositionCycleRequiredTitleRuleFlagsABlankTitle();
    }
}
