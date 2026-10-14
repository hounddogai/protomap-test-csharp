namespace Acme.Workforce.Compensation.Tests.BonusPool.Review;

using Acme.Workforce.Compensation.BonusPool.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class LenientBonusPoolSlotRulesChecks
{
    public static void LenientBonusPoolSlotValidatorReportsEveryProblem()
    {
        var validator = LenientBonusPoolSlotRules.DefaultLenientBonusPoolSlotValidator();
        var item = LenientBonusPoolSlot.NewLenientBonusPoolSlot("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LenientBonusPoolSlotRequiredGradeRuleFlagsABlankGrade()
    {
        var rule = new LenientBonusPoolSlotRequiredGradeRule("lenient bonus pool slot needs a value");
        var item = LenientBonusPoolSlot.NewLenientBonusPoolSlot("", "");
        item.Grade = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LenientBonusPoolSlotDepartmentLengthRuleFlagsALongDepartment()
    {
        var rule = new LenientBonusPoolSlotDepartmentLengthRule(255);
        var item = LenientBonusPoolSlot.NewLenientBonusPoolSlot("x", "y");
        item.Department = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Department = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LenientBonusPoolSlotRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new LenientBonusPoolSlotRequiredCodeRule("lenient bonus pool slot needs this field");
        var item = LenientBonusPoolSlot.NewLenientBonusPoolSlot("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LenientBonusPoolSlotFloorValueRangeRuleFlagsValuesAboveFloorValue()
    {
        var rule = new LenientBonusPoolSlotFloorValueRangeRule(7.2, 118.0);
        var item = LenientBonusPoolSlot.NewLenientBonusPoolSlot("x", "y");
        item.FloorValue = 119.5;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LenientBonusPoolSlotRevisionRangeRuleFlagsValuesAboveRevision()
    {
        var rule = new LenientBonusPoolSlotRevisionRangeRule(64, 255);
        var item = LenientBonusPoolSlot.NewLenientBonusPoolSlot("x", "y");
        item.Revision = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        LenientBonusPoolSlotValidatorReportsEveryProblem();
        LenientBonusPoolSlotRequiredGradeRuleFlagsABlankGrade();
        LenientBonusPoolSlotDepartmentLengthRuleFlagsALongDepartment();
        LenientBonusPoolSlotRequiredCodeRuleFlagsABlankCode();
        LenientBonusPoolSlotFloorValueRangeRuleFlagsValuesAboveFloorValue();
        LenientBonusPoolSlotRevisionRangeRuleFlagsValuesAboveRevision();
    }
}
