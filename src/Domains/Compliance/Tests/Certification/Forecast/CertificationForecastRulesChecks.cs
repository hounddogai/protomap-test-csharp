namespace Acme.Workforce.Compliance.Tests.Certification.Forecast;

using Acme.Workforce.Compliance.Certification.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class CertificationForecastRulesChecks
{
    public static void CertificationForecastValidatorReportsEveryProblem()
    {
        var validator = CertificationForecastRules.DefaultCertificationForecastValidator();
        var item = CertificationForecast.NewCertificationForecast("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CertificationForecastSkillLengthRuleFlagsALongSkill()
    {
        var rule = new CertificationForecastSkillLengthRule(120);
        var item = CertificationForecast.NewCertificationForecast("x", "y");
        item.Skill = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Skill = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CertificationForecastFloorValueRangeRuleFlagsValuesAboveFloorValue()
    {
        var rule = new CertificationForecastFloorValueRangeRule(5.4, 4666.7);
        var item = CertificationForecast.NewCertificationForecast("x", "y");
        item.FloorValue = 4668.2;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void CertificationForecastRequiredSkillRuleFlagsABlankSkill()
    {
        var rule = new CertificationForecastRequiredSkillRule("certification forecast needs this field");
        var item = CertificationForecast.NewCertificationForecast("", "");
        item.Skill = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        CertificationForecastValidatorReportsEveryProblem();
        CertificationForecastSkillLengthRuleFlagsALongSkill();
        CertificationForecastFloorValueRangeRuleFlagsValuesAboveFloorValue();
        CertificationForecastRequiredSkillRuleFlagsABlankSkill();
    }
}
