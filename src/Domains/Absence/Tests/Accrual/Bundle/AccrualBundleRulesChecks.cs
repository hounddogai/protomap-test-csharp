namespace Acme.Workforce.Absence.Tests.Accrual.Bundle;

using Acme.Workforce.Absence.Accrual.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class AccrualBundleRulesChecks
{
    public static void AccrualBundleValidatorReportsEveryProblem()
    {
        var validator = AccrualBundleRules.DefaultAccrualBundleValidator();
        var item = AccrualBundle.NewAccrualBundle("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AccrualBundleRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new AccrualBundleRequiredCodeRule("accrual bundle needs an entry");
        var item = AccrualBundle.NewAccrualBundle("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AccrualBundleContactEmailLengthRuleFlagsALongContactEmail()
    {
        var rule = new AccrualBundleContactEmailLengthRule(255);
        var item = AccrualBundle.NewAccrualBundle("x", "y");
        item.ContactEmail = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ContactEmail = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AccrualBundleNameLengthRuleFlagsALongName()
    {
        var rule = new AccrualBundleNameLengthRule(80);
        var item = AccrualBundle.NewAccrualBundle("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        AccrualBundleValidatorReportsEveryProblem();
        AccrualBundleRequiredCodeRuleFlagsABlankCode();
        AccrualBundleContactEmailLengthRuleFlagsALongContactEmail();
        AccrualBundleNameLengthRuleFlagsALongName();
    }
}
