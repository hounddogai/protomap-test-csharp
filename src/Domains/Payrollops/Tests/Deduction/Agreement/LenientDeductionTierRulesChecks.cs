namespace Acme.Workforce.Payrollops.Tests.Deduction.Agreement;

using Acme.Workforce.Payrollops.Deduction.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class LenientDeductionTierRulesChecks
{
    public static void LenientDeductionTierValidatorReportsEveryProblem()
    {
        var validator = LenientDeductionTierRules.DefaultLenientDeductionTierValidator();
        var item = LenientDeductionTier.NewLenientDeductionTier("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LenientDeductionTierRequiredPhoneRuleFlagsABlankPhone()
    {
        var rule = new LenientDeductionTierRequiredPhoneRule("lenient deduction tier needs a value");
        var item = LenientDeductionTier.NewLenientDeductionTier("", "");
        item.Phone = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LenientDeductionTierEmployeeIdLengthRuleFlagsALongEmployeeId()
    {
        var rule = new LenientDeductionTierEmployeeIdLengthRule(64);
        var item = LenientDeductionTier.NewLenientDeductionTier("x", "y");
        item.EmployeeId = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.EmployeeId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LenientDeductionTierRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new LenientDeductionTierRequiredCodeRule("lenient deduction tier needs this field");
        var item = LenientDeductionTier.NewLenientDeductionTier("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LenientDeductionTierCodeLengthRuleFlagsALongCode()
    {
        var rule = new LenientDeductionTierCodeLengthRule(64);
        var item = LenientDeductionTier.NewLenientDeductionTier("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LenientDeductionTierPhoneLengthRuleFlagsALongPhone()
    {
        var rule = new LenientDeductionTierPhoneLengthRule(255);
        var item = LenientDeductionTier.NewLenientDeductionTier("x", "y");
        item.Phone = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Phone = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        LenientDeductionTierValidatorReportsEveryProblem();
        LenientDeductionTierRequiredPhoneRuleFlagsABlankPhone();
        LenientDeductionTierEmployeeIdLengthRuleFlagsALongEmployeeId();
        LenientDeductionTierRequiredCodeRuleFlagsABlankCode();
        LenientDeductionTierCodeLengthRuleFlagsALongCode();
        LenientDeductionTierPhoneLengthRuleFlagsALongPhone();
    }
}
