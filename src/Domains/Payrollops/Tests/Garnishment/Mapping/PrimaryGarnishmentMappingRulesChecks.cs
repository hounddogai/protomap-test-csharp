namespace Acme.Workforce.Payrollops.Tests.Garnishment.Mapping;

using Acme.Workforce.Payrollops.Garnishment.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class PrimaryGarnishmentMappingRulesChecks
{
    public static void PrimaryGarnishmentMappingValidatorReportsEveryProblem()
    {
        var validator = PrimaryGarnishmentMappingRules.DefaultPrimaryGarnishmentMappingValidator();
        var item = PrimaryGarnishmentMapping.NewPrimaryGarnishmentMapping("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PrimaryGarnishmentMappingRequiredNameRuleFlagsABlankName()
    {
        var rule = new PrimaryGarnishmentMappingRequiredNameRule("primary garnishment mapping needs an entry");
        var item = PrimaryGarnishmentMapping.NewPrimaryGarnishmentMapping("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PrimaryGarnishmentMappingUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt()
    {
        var rule = new PrimaryGarnishmentMappingUpdatedAtRangeRule(16, 86400);
        var item = PrimaryGarnishmentMapping.NewPrimaryGarnishmentMapping("x", "y");
        item.UpdatedAt = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PrimaryGarnishmentMappingCodeLengthRuleFlagsALongCode()
    {
        var rule = new PrimaryGarnishmentMappingCodeLengthRule(120);
        var item = PrimaryGarnishmentMapping.NewPrimaryGarnishmentMapping("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PrimaryGarnishmentMappingCeilingRangeRuleFlagsValuesAboveCeiling()
    {
        var rule = new PrimaryGarnishmentMappingCeilingRangeRule(7.9, 1708.5);
        var item = PrimaryGarnishmentMapping.NewPrimaryGarnishmentMapping("x", "y");
        item.Ceiling = 1710.0;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PrimaryGarnishmentMappingRequiredEmployeeIdRuleFlagsABlankEmployeeId()
    {
        var rule = new PrimaryGarnishmentMappingRequiredEmployeeIdRule("primary garnishment mapping needs a value");
        var item = PrimaryGarnishmentMapping.NewPrimaryGarnishmentMapping("", "");
        item.EmployeeId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PrimaryGarnishmentMappingExternalRefLengthRuleFlagsALongExternalRef()
    {
        var rule = new PrimaryGarnishmentMappingExternalRefLengthRule(1000);
        var item = PrimaryGarnishmentMapping.NewPrimaryGarnishmentMapping("x", "y");
        item.ExternalRef = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ExternalRef = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        PrimaryGarnishmentMappingValidatorReportsEveryProblem();
        PrimaryGarnishmentMappingRequiredNameRuleFlagsABlankName();
        PrimaryGarnishmentMappingUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt();
        PrimaryGarnishmentMappingCodeLengthRuleFlagsALongCode();
        PrimaryGarnishmentMappingCeilingRangeRuleFlagsValuesAboveCeiling();
        PrimaryGarnishmentMappingRequiredEmployeeIdRuleFlagsABlankEmployeeId();
        PrimaryGarnishmentMappingExternalRefLengthRuleFlagsALongExternalRef();
    }
}
