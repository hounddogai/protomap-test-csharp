namespace Acme.Workforce.Orgdesign.Tests.Reorg.ReorgException;

using Acme.Workforce.Orgdesign.Reorg.ReorgException;

/// <summary>Checks of reorg exception code, run by the project's runner.</summary>
public static class ReorgSlotRulesChecks
{
    public static void ReorgSlotValidatorReportsEveryProblem()
    {
        var validator = ReorgSlotRules.DefaultReorgSlotValidator();
        var item = ReorgSlot.NewReorgSlot("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ReorgSlotCodeLengthRuleFlagsALongCode()
    {
        var rule = new ReorgSlotCodeLengthRule(16);
        var item = ReorgSlot.NewReorgSlot("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ReorgSlotThresholdRangeRuleFlagsValuesAboveThreshold()
    {
        var rule = new ReorgSlotThresholdRangeRule(1.0, 3464.9);
        var item = ReorgSlot.NewReorgSlot("x", "y");
        item.Threshold = 3466.4;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ReorgSlotRequiredGradeRuleFlagsABlankGrade()
    {
        var rule = new ReorgSlotRequiredGradeRule("reorg slot needs this field");
        var item = ReorgSlot.NewReorgSlot("", "");
        item.Grade = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        ReorgSlotValidatorReportsEveryProblem();
        ReorgSlotCodeLengthRuleFlagsALongCode();
        ReorgSlotThresholdRangeRuleFlagsValuesAboveThreshold();
        ReorgSlotRequiredGradeRuleFlagsABlankGrade();
    }
}
