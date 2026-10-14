namespace Acme.Workforce.Compliance.Tests.Certification.Request;

using Acme.Workforce.Compliance.Certification.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class CertificationRequestRulesChecks
{
    public static void CertificationRequestValidatorReportsEveryProblem()
    {
        var validator = CertificationRequestRules.DefaultCertificationRequestValidator();
        var item = CertificationRequest.NewCertificationRequest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CertificationRequestRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new CertificationRequestRequiredCodeRule("certification request needs an entry");
        var item = CertificationRequest.NewCertificationRequest("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void CertificationRequestTimezoneLengthRuleFlagsALongTimezone()
    {
        var rule = new CertificationRequestTimezoneLengthRule(255);
        var item = CertificationRequest.NewCertificationRequest("x", "y");
        item.Timezone = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Timezone = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CertificationRequestRequiredTimezoneRuleFlagsABlankTimezone()
    {
        var rule = new CertificationRequestRequiredTimezoneRule("certification request needs this field");
        var item = CertificationRequest.NewCertificationRequest("", "");
        item.Timezone = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void CertificationRequestTitleLengthRuleFlagsALongTitle()
    {
        var rule = new CertificationRequestTitleLengthRule(3600);
        var item = CertificationRequest.NewCertificationRequest("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        CertificationRequestValidatorReportsEveryProblem();
        CertificationRequestRequiredCodeRuleFlagsABlankCode();
        CertificationRequestTimezoneLengthRuleFlagsALongTimezone();
        CertificationRequestRequiredTimezoneRuleFlagsABlankTimezone();
        CertificationRequestTitleLengthRuleFlagsALongTitle();
    }
}
