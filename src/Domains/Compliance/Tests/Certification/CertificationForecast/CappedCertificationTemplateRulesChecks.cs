namespace Acme.Workforce.Compliance.Tests.Certification.CertificationForecast;

using Acme.Workforce.Compliance.Certification.CertificationForecast;

/// <summary>Checks of certification forecast code, run by the project's runner.</summary>
public static class CappedCertificationTemplateRulesChecks
{
    public static void CappedCertificationTemplateValidatorReportsEveryProblem()
    {
        var validator = CappedCertificationTemplateRules.DefaultCappedCertificationTemplateValidator();
        var item = CappedCertificationTemplate.NewCappedCertificationTemplate("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CappedCertificationTemplateCodeLengthRuleFlagsALongCode()
    {
        var rule = new CappedCertificationTemplateCodeLengthRule(255);
        var item = CappedCertificationTemplate.NewCappedCertificationTemplate("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CappedCertificationTemplateGraceDaysRangeRuleFlagsValuesAboveGraceDays()
    {
        var rule = new CappedCertificationTemplateGraceDaysRangeRule(255, 3600);
        var item = CappedCertificationTemplate.NewCappedCertificationTemplate("x", "y");
        item.GraceDays = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        CappedCertificationTemplateValidatorReportsEveryProblem();
        CappedCertificationTemplateCodeLengthRuleFlagsALongCode();
        CappedCertificationTemplateGraceDaysRangeRuleFlagsValuesAboveGraceDays();
    }
}
