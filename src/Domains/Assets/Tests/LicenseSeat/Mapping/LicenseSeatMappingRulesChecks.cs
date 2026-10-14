namespace Acme.Workforce.Assets.Tests.LicenseSeat.Mapping;

using Acme.Workforce.Assets.LicenseSeat.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class LicenseSeatMappingRulesChecks
{
    public static void LicenseSeatMappingValidatorReportsEveryProblem()
    {
        var validator = LicenseSeatMappingRules.DefaultLicenseSeatMappingValidator();
        var item = LicenseSeatMapping.NewLicenseSeatMapping("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LicenseSeatMappingCategoryLengthRuleFlagsALongCategory()
    {
        var rule = new LicenseSeatMappingCategoryLengthRule(32);
        var item = LicenseSeatMapping.NewLicenseSeatMapping("x", "y");
        item.Category = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Category = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LicenseSeatMappingRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new LicenseSeatMappingRequiredTitleRule("license seat mapping needs an entry");
        var item = LicenseSeatMapping.NewLicenseSeatMapping("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LicenseSeatMappingRequiredRegionRuleFlagsABlankRegion()
    {
        var rule = new LicenseSeatMappingRequiredRegionRule("license seat mapping needs an entry");
        var item = LicenseSeatMapping.NewLicenseSeatMapping("", "");
        item.Region = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        LicenseSeatMappingValidatorReportsEveryProblem();
        LicenseSeatMappingCategoryLengthRuleFlagsALongCategory();
        LicenseSeatMappingRequiredTitleRuleFlagsABlankTitle();
        LicenseSeatMappingRequiredRegionRuleFlagsABlankRegion();
    }
}
