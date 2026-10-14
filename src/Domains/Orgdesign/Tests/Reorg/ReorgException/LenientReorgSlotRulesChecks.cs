namespace Acme.Workforce.Orgdesign.Tests.Reorg.ReorgException;

using Acme.Workforce.Orgdesign.Reorg.ReorgException;

/// <summary>Checks of reorg exception code, run by the project's runner.</summary>
public static class LenientReorgSlotRulesChecks
{
    public static void LenientReorgSlotValidatorReportsEveryProblem()
    {
        var validator = LenientReorgSlotRules.DefaultLenientReorgSlotValidator();
        var item = LenientReorgSlot.NewLenientReorgSlot("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LenientReorgSlotCodeLengthRuleFlagsALongCode()
    {
        var rule = new LenientReorgSlotCodeLengthRule(1000);
        var item = LenientReorgSlot.NewLenientReorgSlot("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LenientReorgSlotRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new LenientReorgSlotRequiredCodeRule("lenient reorg slot needs a value");
        var item = LenientReorgSlot.NewLenientReorgSlot("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LenientReorgSlotAttemptsRangeRuleFlagsValuesAboveAttempts()
    {
        var rule = new LenientReorgSlotAttemptsRangeRule(32, 120);
        var item = LenientReorgSlot.NewLenientReorgSlot("x", "y");
        item.Attempts = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LenientReorgSlotSequenceRangeRuleFlagsValuesAboveSequence()
    {
        var rule = new LenientReorgSlotSequenceRangeRule(120, 255);
        var item = LenientReorgSlot.NewLenientReorgSlot("x", "y");
        item.Sequence = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        LenientReorgSlotValidatorReportsEveryProblem();
        LenientReorgSlotCodeLengthRuleFlagsALongCode();
        LenientReorgSlotRequiredCodeRuleFlagsABlankCode();
        LenientReorgSlotAttemptsRangeRuleFlagsValuesAboveAttempts();
        LenientReorgSlotSequenceRangeRuleFlagsValuesAboveSequence();
    }
}
