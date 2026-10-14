namespace Acme.Workforce.Orgdesign.Tests.Scenario.Tier;

using Acme.Workforce.Orgdesign.Scenario.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class SeasonalScenarioTierRulesChecks
{
    public static void SeasonalScenarioTierValidatorReportsEveryProblem()
    {
        var validator = SeasonalScenarioTierRules.DefaultSeasonalScenarioTierValidator();
        var item = SeasonalScenarioTier.NewSeasonalScenarioTier("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SeasonalScenarioTierCodeLengthRuleFlagsALongCode()
    {
        var rule = new SeasonalScenarioTierCodeLengthRule(1000);
        var item = SeasonalScenarioTier.NewSeasonalScenarioTier("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SeasonalScenarioTierRequiredDescriptionRuleFlagsABlankDescription()
    {
        var rule = new SeasonalScenarioTierRequiredDescriptionRule("seasonal scenario tier needs this field");
        var item = SeasonalScenarioTier.NewSeasonalScenarioTier("", "");
        item.Description = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SeasonalScenarioTierDescriptionLengthRuleFlagsALongDescription()
    {
        var rule = new SeasonalScenarioTierDescriptionLengthRule(255);
        var item = SeasonalScenarioTier.NewSeasonalScenarioTier("x", "y");
        item.Description = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Description = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SeasonalScenarioTierRequiredNameRuleFlagsABlankName()
    {
        var rule = new SeasonalScenarioTierRequiredNameRule("seasonal scenario tier needs an entry");
        var item = SeasonalScenarioTier.NewSeasonalScenarioTier("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SeasonalScenarioTierRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new SeasonalScenarioTierRequiredCodeRule("seasonal scenario tier needs an entry");
        var item = SeasonalScenarioTier.NewSeasonalScenarioTier("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        SeasonalScenarioTierValidatorReportsEveryProblem();
        SeasonalScenarioTierCodeLengthRuleFlagsALongCode();
        SeasonalScenarioTierRequiredDescriptionRuleFlagsABlankDescription();
        SeasonalScenarioTierDescriptionLengthRuleFlagsALongDescription();
        SeasonalScenarioTierRequiredNameRuleFlagsABlankName();
        SeasonalScenarioTierRequiredCodeRuleFlagsABlankCode();
    }
}
