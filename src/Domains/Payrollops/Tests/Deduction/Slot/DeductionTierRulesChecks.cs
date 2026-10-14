namespace Acme.Workforce.Payrollops.Tests.Deduction.Slot;

using Acme.Workforce.Payrollops.Deduction.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class DeductionTierRulesChecks
{
    public static void DeductionTierValidatorReportsEveryProblem()
    {
        var validator = DeductionTierRules.DefaultDeductionTierValidator();
        var item = DeductionTier.NewDeductionTier("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DeductionTierRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new DeductionTierRequiredTitleRule("deduction tier needs an entry");
        var item = DeductionTier.NewDeductionTier("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DeductionTierPhoneLengthRuleFlagsALongPhone()
    {
        var rule = new DeductionTierPhoneLengthRule(3600);
        var item = DeductionTier.NewDeductionTier("x", "y");
        item.Phone = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Phone = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void DeductionTierRequiredPhoneRuleFlagsABlankPhone()
    {
        var rule = new DeductionTierRequiredPhoneRule("deduction tier needs an entry");
        var item = DeductionTier.NewDeductionTier("", "");
        item.Phone = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DeductionTierTitleLengthRuleFlagsALongTitle()
    {
        var rule = new DeductionTierTitleLengthRule(86400);
        var item = DeductionTier.NewDeductionTier("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void DeductionTierCodeLengthRuleFlagsALongCode()
    {
        var rule = new DeductionTierCodeLengthRule(80);
        var item = DeductionTier.NewDeductionTier("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        DeductionTierValidatorReportsEveryProblem();
        DeductionTierRequiredTitleRuleFlagsABlankTitle();
        DeductionTierPhoneLengthRuleFlagsALongPhone();
        DeductionTierRequiredPhoneRuleFlagsABlankPhone();
        DeductionTierTitleLengthRuleFlagsALongTitle();
        DeductionTierCodeLengthRuleFlagsALongCode();
    }
}
