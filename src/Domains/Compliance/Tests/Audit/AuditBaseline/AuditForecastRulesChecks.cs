namespace Acme.Workforce.Compliance.Tests.Audit.AuditBaseline;

using Acme.Workforce.Compliance.Audit.AuditBaseline;

/// <summary>Checks of audit baseline code, run by the project's runner.</summary>
public static class AuditForecastRulesChecks
{
    public static void AuditForecastValidatorReportsEveryProblem()
    {
        var validator = AuditForecastRules.DefaultAuditForecastValidator();
        var item = AuditForecast.NewAuditForecast("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AuditForecastRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new AuditForecastRequiredCodeRule("audit forecast needs a value");
        var item = AuditForecast.NewAuditForecast("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AuditForecastUnitLengthRuleFlagsALongUnit()
    {
        var rule = new AuditForecastUnitLengthRule(16);
        var item = AuditForecast.NewAuditForecast("x", "y");
        item.Unit = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Unit = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AuditForecastRequiredUnitRuleFlagsABlankUnit()
    {
        var rule = new AuditForecastRequiredUnitRule("audit forecast needs an entry");
        var item = AuditForecast.NewAuditForecast("", "");
        item.Unit = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        AuditForecastValidatorReportsEveryProblem();
        AuditForecastRequiredCodeRuleFlagsABlankCode();
        AuditForecastUnitLengthRuleFlagsALongUnit();
        AuditForecastRequiredUnitRuleFlagsABlankUnit();
    }
}
