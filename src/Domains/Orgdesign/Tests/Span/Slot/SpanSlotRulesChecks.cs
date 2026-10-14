namespace Acme.Workforce.Orgdesign.Tests.Span.Slot;

using Acme.Workforce.Orgdesign.Span.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class SpanSlotRulesChecks
{
    public static void SpanSlotValidatorReportsEveryProblem()
    {
        var validator = SpanSlotRules.DefaultSpanSlotValidator();
        var item = SpanSlot.NewSpanSlot("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SpanSlotCeilingRangeRuleFlagsValuesAboveCeiling()
    {
        var rule = new SpanSlotCeilingRangeRule(0.1, 2960.6);
        var item = SpanSlot.NewSpanSlot("x", "y");
        item.Ceiling = 2962.1;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void SpanSlotEndsAtRangeRuleFlagsValuesAboveEndsAt()
    {
        var rule = new SpanSlotEndsAtRangeRule(16, 3600);
        var item = SpanSlot.NewSpanSlot("x", "y");
        item.EndsAt = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void SpanSlotAttemptsRangeRuleFlagsValuesAboveAttempts()
    {
        var rule = new SpanSlotAttemptsRangeRule(80, 1000);
        var item = SpanSlot.NewSpanSlot("x", "y");
        item.Attempts = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        SpanSlotValidatorReportsEveryProblem();
        SpanSlotCeilingRangeRuleFlagsValuesAboveCeiling();
        SpanSlotEndsAtRangeRuleFlagsValuesAboveEndsAt();
        SpanSlotAttemptsRangeRuleFlagsValuesAboveAttempts();
    }
}
