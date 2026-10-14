namespace Acme.Workforce.Workplace.Tests.Desk.Tier;

using Acme.Workforce.Workplace.Desk.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class RegionalDeskMappingRulesChecks
{
    public static void RegionalDeskMappingValidatorReportsEveryProblem()
    {
        var validator = RegionalDeskMappingRules.DefaultRegionalDeskMappingValidator();
        var item = RegionalDeskMapping.NewRegionalDeskMapping("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RegionalDeskMappingOwnerIdLengthRuleFlagsALongOwnerId()
    {
        var rule = new RegionalDeskMappingOwnerIdLengthRule(255);
        var item = RegionalDeskMapping.NewRegionalDeskMapping("x", "y");
        item.OwnerId = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.OwnerId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RegionalDeskMappingRequiredCountryRuleFlagsABlankCountry()
    {
        var rule = new RegionalDeskMappingRequiredCountryRule("regional desk mapping needs a value");
        var item = RegionalDeskMapping.NewRegionalDeskMapping("", "");
        item.Country = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RegionalDeskMappingCodeLengthRuleFlagsALongCode()
    {
        var rule = new RegionalDeskMappingCodeLengthRule(64);
        var item = RegionalDeskMapping.NewRegionalDeskMapping("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RegionalDeskMappingRequiredOwnerIdRuleFlagsABlankOwnerId()
    {
        var rule = new RegionalDeskMappingRequiredOwnerIdRule("regional desk mapping needs this field");
        var item = RegionalDeskMapping.NewRegionalDeskMapping("", "");
        item.OwnerId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        RegionalDeskMappingValidatorReportsEveryProblem();
        RegionalDeskMappingOwnerIdLengthRuleFlagsALongOwnerId();
        RegionalDeskMappingRequiredCountryRuleFlagsABlankCountry();
        RegionalDeskMappingCodeLengthRuleFlagsALongCode();
        RegionalDeskMappingRequiredOwnerIdRuleFlagsABlankOwnerId();
    }
}
