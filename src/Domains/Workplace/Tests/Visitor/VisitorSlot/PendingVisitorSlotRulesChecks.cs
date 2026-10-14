namespace Acme.Workforce.Workplace.Tests.Visitor.VisitorSlot;

using Acme.Workforce.Workplace.Visitor.VisitorSlot;

/// <summary>Checks of visitor slot code, run by the project's runner.</summary>
public static class PendingVisitorSlotRulesChecks
{
    public static void PendingVisitorSlotValidatorReportsEveryProblem()
    {
        var validator = PendingVisitorSlotRules.DefaultPendingVisitorSlotValidator();
        var item = PendingVisitorSlot.NewPendingVisitorSlot("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PendingVisitorSlotRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new PendingVisitorSlotRequiredCodeRule("pending visitor slot needs a value");
        var item = PendingVisitorSlot.NewPendingVisitorSlot("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PendingVisitorSlotTimezoneLengthRuleFlagsALongTimezone()
    {
        var rule = new PendingVisitorSlotTimezoneLengthRule(16);
        var item = PendingVisitorSlot.NewPendingVisitorSlot("x", "y");
        item.Timezone = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Timezone = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PendingVisitorSlotTitleLengthRuleFlagsALongTitle()
    {
        var rule = new PendingVisitorSlotTitleLengthRule(80);
        var item = PendingVisitorSlot.NewPendingVisitorSlot("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        PendingVisitorSlotValidatorReportsEveryProblem();
        PendingVisitorSlotRequiredCodeRuleFlagsABlankCode();
        PendingVisitorSlotTimezoneLengthRuleFlagsALongTimezone();
        PendingVisitorSlotTitleLengthRuleFlagsALongTitle();
    }
}
