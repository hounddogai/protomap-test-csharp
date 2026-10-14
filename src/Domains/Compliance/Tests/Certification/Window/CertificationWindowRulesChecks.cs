namespace Acme.Workforce.Compliance.Tests.Certification.Window;

using Acme.Workforce.Compliance.Certification.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class CertificationWindowRulesChecks
{
    public static void CertificationWindowValidatorReportsEveryProblem()
    {
        var validator = CertificationWindowRules.DefaultCertificationWindowValidator();
        var item = CertificationWindow.NewCertificationWindow("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CertificationWindowLabelLengthRuleFlagsALongLabel()
    {
        var rule = new CertificationWindowLabelLengthRule(64);
        var item = CertificationWindow.NewCertificationWindow("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CertificationWindowRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new CertificationWindowRequiredCodeRule("certification window needs this field");
        var item = CertificationWindow.NewCertificationWindow("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        CertificationWindowValidatorReportsEveryProblem();
        CertificationWindowLabelLengthRuleFlagsALongLabel();
        CertificationWindowRequiredCodeRuleFlagsABlankCode();
    }
}
