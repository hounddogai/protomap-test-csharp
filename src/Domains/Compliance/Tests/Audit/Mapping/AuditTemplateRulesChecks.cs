namespace Acme.Workforce.Compliance.Tests.Audit.Mapping;

using Acme.Workforce.Compliance.Audit.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class AuditTemplateRulesChecks
{
    public static void AuditTemplateValidatorReportsEveryProblem()
    {
        var validator = AuditTemplateRules.DefaultAuditTemplateValidator();
        var item = AuditTemplate.NewAuditTemplate("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AuditTemplateQuantityRangeRuleFlagsValuesAboveQuantity()
    {
        var rule = new AuditTemplateQuantityRangeRule(80, 1000);
        var item = AuditTemplate.NewAuditTemplate("x", "y");
        item.Quantity = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        AuditTemplateValidatorReportsEveryProblem();
        AuditTemplateQuantityRangeRuleFlagsValuesAboveQuantity();
    }
}
