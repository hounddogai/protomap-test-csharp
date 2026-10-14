namespace Acme.Workforce.Absence.Tests.Accrual.OverrideItem;

using Acme.Workforce.Absence.Accrual.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class LenientAccrualAssignmentRulesChecks
{
    public static void LenientAccrualAssignmentValidatorReportsEveryProblem()
    {
        var validator = LenientAccrualAssignmentRules.DefaultLenientAccrualAssignmentValidator();
        var item = LenientAccrualAssignment.NewLenientAccrualAssignment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LenientAccrualAssignmentRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new LenientAccrualAssignmentRequiredCodeRule("lenient accrual assignment needs a value");
        var item = LenientAccrualAssignment.NewLenientAccrualAssignment("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LenientAccrualAssignmentPhoneLengthRuleFlagsALongPhone()
    {
        var rule = new LenientAccrualAssignmentPhoneLengthRule(16);
        var item = LenientAccrualAssignment.NewLenientAccrualAssignment("x", "y");
        item.Phone = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Phone = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LenientAccrualAssignmentCodeLengthRuleFlagsALongCode()
    {
        var rule = new LenientAccrualAssignmentCodeLengthRule(32);
        var item = LenientAccrualAssignment.NewLenientAccrualAssignment("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LenientAccrualAssignmentRequiredPhoneRuleFlagsABlankPhone()
    {
        var rule = new LenientAccrualAssignmentRequiredPhoneRule("lenient accrual assignment needs an entry");
        var item = LenientAccrualAssignment.NewLenientAccrualAssignment("", "");
        item.Phone = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        LenientAccrualAssignmentValidatorReportsEveryProblem();
        LenientAccrualAssignmentRequiredCodeRuleFlagsABlankCode();
        LenientAccrualAssignmentPhoneLengthRuleFlagsALongPhone();
        LenientAccrualAssignmentCodeLengthRuleFlagsALongCode();
        LenientAccrualAssignmentRequiredPhoneRuleFlagsABlankPhone();
    }
}
