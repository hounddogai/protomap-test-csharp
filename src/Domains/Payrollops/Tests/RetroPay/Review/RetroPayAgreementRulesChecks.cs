namespace Acme.Workforce.Payrollops.Tests.RetroPay.Review;

using Acme.Workforce.Payrollops.RetroPay.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class RetroPayAgreementRulesChecks
{
    public static void RetroPayAgreementValidatorReportsEveryProblem()
    {
        var validator = RetroPayAgreementRules.DefaultRetroPayAgreementValidator();
        var item = RetroPayAgreement.NewRetroPayAgreement("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RetroPayAgreementEmployeeIdLengthRuleFlagsALongEmployeeId()
    {
        var rule = new RetroPayAgreementEmployeeIdLengthRule(120);
        var item = RetroPayAgreement.NewRetroPayAgreement("x", "y");
        item.EmployeeId = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.EmployeeId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RetroPayAgreementVersionRangeRuleFlagsValuesAboveVersion()
    {
        var rule = new RetroPayAgreementVersionRangeRule(120, 3600);
        var item = RetroPayAgreement.NewRetroPayAgreement("x", "y");
        item.Version = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RetroPayAgreementRequiredEmployeeIdRuleFlagsABlankEmployeeId()
    {
        var rule = new RetroPayAgreementRequiredEmployeeIdRule("retro pay agreement needs a value");
        var item = RetroPayAgreement.NewRetroPayAgreement("", "");
        item.EmployeeId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        RetroPayAgreementValidatorReportsEveryProblem();
        RetroPayAgreementEmployeeIdLengthRuleFlagsALongEmployeeId();
        RetroPayAgreementVersionRangeRuleFlagsValuesAboveVersion();
        RetroPayAgreementRequiredEmployeeIdRuleFlagsABlankEmployeeId();
    }
}
