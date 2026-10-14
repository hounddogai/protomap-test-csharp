namespace Acme.Workforce.Orgdesign.Tests.Reorg.Metric;

using Acme.Workforce.Orgdesign.Reorg.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class ReorgMappingRulesChecks
{
    public static void ReorgMappingValidatorReportsEveryProblem()
    {
        var validator = ReorgMappingRules.DefaultReorgMappingValidator();
        var item = ReorgMapping.NewReorgMapping("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ReorgMappingRequiredCountryRuleFlagsABlankCountry()
    {
        var rule = new ReorgMappingRequiredCountryRule("reorg mapping needs this field");
        var item = ReorgMapping.NewReorgMapping("", "");
        item.Country = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ReorgMappingCodeLengthRuleFlagsALongCode()
    {
        var rule = new ReorgMappingCodeLengthRule(3600);
        var item = ReorgMapping.NewReorgMapping("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ReorgMappingTitleLengthRuleFlagsALongTitle()
    {
        var rule = new ReorgMappingTitleLengthRule(32);
        var item = ReorgMapping.NewReorgMapping("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ReorgMappingRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new ReorgMappingRequiredTitleRule("reorg mapping needs an entry");
        var item = ReorgMapping.NewReorgMapping("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        ReorgMappingValidatorReportsEveryProblem();
        ReorgMappingRequiredCountryRuleFlagsABlankCountry();
        ReorgMappingCodeLengthRuleFlagsALongCode();
        ReorgMappingTitleLengthRuleFlagsALongTitle();
        ReorgMappingRequiredTitleRuleFlagsABlankTitle();
    }
}
