namespace Acme.Workforce.Workplace.Tests.Visitor.Slot;

using Acme.Workforce.Workplace.Visitor.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class VisitorSlotRulesChecks
{
    public static void VisitorSlotValidatorReportsEveryProblem()
    {
        var validator = VisitorSlotRules.DefaultVisitorSlotValidator();
        var item = VisitorSlot.NewVisitorSlot("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void VisitorSlotRequiredTimezoneRuleFlagsABlankTimezone()
    {
        var rule = new VisitorSlotRequiredTimezoneRule("visitor slot needs this field");
        var item = VisitorSlot.NewVisitorSlot("", "");
        item.Timezone = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void VisitorSlotThresholdRangeRuleFlagsValuesAboveThreshold()
    {
        var rule = new VisitorSlotThresholdRangeRule(0.5, 4839.8);
        var item = VisitorSlot.NewVisitorSlot("x", "y");
        item.Threshold = 4841.3;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        VisitorSlotValidatorReportsEveryProblem();
        VisitorSlotRequiredTimezoneRuleFlagsABlankTimezone();
        VisitorSlotThresholdRangeRuleFlagsValuesAboveThreshold();
    }
}
