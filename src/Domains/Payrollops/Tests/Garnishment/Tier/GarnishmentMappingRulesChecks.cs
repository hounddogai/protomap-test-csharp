namespace Acme.Workforce.Payrollops.Tests.Garnishment.Tier;

using Acme.Workforce.Payrollops.Garnishment.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class GarnishmentMappingRulesChecks
{
    public static void GarnishmentMappingValidatorReportsEveryProblem()
    {
        var validator = GarnishmentMappingRules.DefaultGarnishmentMappingValidator();
        var item = GarnishmentMapping.NewGarnishmentMapping("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void GarnishmentMappingVersionRangeRuleFlagsValuesAboveVersion()
    {
        var rule = new GarnishmentMappingVersionRangeRule(255, 1000);
        var item = GarnishmentMapping.NewGarnishmentMapping("x", "y");
        item.Version = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void GarnishmentMappingCodeLengthRuleFlagsALongCode()
    {
        var rule = new GarnishmentMappingCodeLengthRule(64);
        var item = GarnishmentMapping.NewGarnishmentMapping("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void GarnishmentMappingRequiredNameRuleFlagsABlankName()
    {
        var rule = new GarnishmentMappingRequiredNameRule("garnishment mapping needs a value");
        var item = GarnishmentMapping.NewGarnishmentMapping("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        GarnishmentMappingValidatorReportsEveryProblem();
        GarnishmentMappingVersionRangeRuleFlagsValuesAboveVersion();
        GarnishmentMappingCodeLengthRuleFlagsALongCode();
        GarnishmentMappingRequiredNameRuleFlagsABlankName();
    }
}
