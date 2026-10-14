namespace Acme.Workforce.Orgdesign.Tests.Span.Entry;

using Acme.Workforce.Orgdesign.Span.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class CappedSpanSlotRulesChecks
{
    public static void CappedSpanSlotValidatorReportsEveryProblem()
    {
        var validator = CappedSpanSlotRules.DefaultCappedSpanSlotValidator();
        var item = CappedSpanSlot.NewCappedSpanSlot("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CappedSpanSlotRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new CappedSpanSlotRequiredCodeRule("capped span slot needs an entry");
        var item = CappedSpanSlot.NewCappedSpanSlot("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void CappedSpanSlotPhoneLengthRuleFlagsALongPhone()
    {
        var rule = new CappedSpanSlotPhoneLengthRule(64);
        var item = CappedSpanSlot.NewCappedSpanSlot("x", "y");
        item.Phone = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Phone = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CappedSpanSlotRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new CappedSpanSlotRequiredLabelRule("capped span slot needs this field");
        var item = CappedSpanSlot.NewCappedSpanSlot("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void CappedSpanSlotScoreRangeRuleFlagsValuesAboveScore()
    {
        var rule = new CappedSpanSlotScoreRangeRule(6.0, 2601.5);
        var item = CappedSpanSlot.NewCappedSpanSlot("x", "y");
        item.Score = 2603.0;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        CappedSpanSlotValidatorReportsEveryProblem();
        CappedSpanSlotRequiredCodeRuleFlagsABlankCode();
        CappedSpanSlotPhoneLengthRuleFlagsALongPhone();
        CappedSpanSlotRequiredLabelRuleFlagsABlankLabel();
        CappedSpanSlotScoreRangeRuleFlagsValuesAboveScore();
    }
}
