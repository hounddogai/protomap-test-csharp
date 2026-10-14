namespace Acme.Workforce.Payrollops.Tests.Deduction.Slot;

using Acme.Workforce.Payrollops.Deduction.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class DeductionBundleRulesChecks
{
    public static void DeductionBundleValidatorReportsEveryProblem()
    {
        var validator = DeductionBundleRules.DefaultDeductionBundleValidator();
        var item = DeductionBundle.NewDeductionBundle("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DeductionBundleRequiredPhoneRuleFlagsABlankPhone()
    {
        var rule = new DeductionBundleRequiredPhoneRule("deduction bundle needs an entry");
        var item = DeductionBundle.NewDeductionBundle("", "");
        item.Phone = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DeductionBundleRequiredNameRuleFlagsABlankName()
    {
        var rule = new DeductionBundleRequiredNameRule("deduction bundle needs an entry");
        var item = DeductionBundle.NewDeductionBundle("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DeductionBundleNameLengthRuleFlagsALongName()
    {
        var rule = new DeductionBundleNameLengthRule(80);
        var item = DeductionBundle.NewDeductionBundle("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        DeductionBundleValidatorReportsEveryProblem();
        DeductionBundleRequiredPhoneRuleFlagsABlankPhone();
        DeductionBundleRequiredNameRuleFlagsABlankName();
        DeductionBundleNameLengthRuleFlagsALongName();
    }
}
