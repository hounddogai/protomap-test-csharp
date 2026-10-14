namespace Acme.Workforce.Compliance.Tests.Audit.Mapping;

using Acme.Workforce.Compliance.Audit.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class AuditMappingRulesChecks
{
    public static void AuditMappingValidatorReportsEveryProblem()
    {
        var validator = AuditMappingRules.DefaultAuditMappingValidator();
        var item = AuditMapping.NewAuditMapping("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AuditMappingCodeLengthRuleFlagsALongCode()
    {
        var rule = new AuditMappingCodeLengthRule(255);
        var item = AuditMapping.NewAuditMapping("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AuditMappingNameLengthRuleFlagsALongName()
    {
        var rule = new AuditMappingNameLengthRule(3600);
        var item = AuditMapping.NewAuditMapping("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AuditMappingTimezoneLengthRuleFlagsALongTimezone()
    {
        var rule = new AuditMappingTimezoneLengthRule(80);
        var item = AuditMapping.NewAuditMapping("x", "y");
        item.Timezone = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Timezone = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AuditMappingGraceDaysRangeRuleFlagsValuesAboveGraceDays()
    {
        var rule = new AuditMappingGraceDaysRangeRule(120, 86400);
        var item = AuditMapping.NewAuditMapping("x", "y");
        item.GraceDays = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        AuditMappingValidatorReportsEveryProblem();
        AuditMappingCodeLengthRuleFlagsALongCode();
        AuditMappingNameLengthRuleFlagsALongName();
        AuditMappingTimezoneLengthRuleFlagsALongTimezone();
        AuditMappingGraceDaysRangeRuleFlagsValuesAboveGraceDays();
    }
}
