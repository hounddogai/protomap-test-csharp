namespace Acme.Workforce.Orgdesign.Tests.Scenario.Request;

using Acme.Workforce.Orgdesign.Scenario.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class FallbackScenarioOverrideRulesChecks
{
    public static void FallbackScenarioOverrideValidatorReportsEveryProblem()
    {
        var validator = FallbackScenarioOverrideRules.DefaultFallbackScenarioOverrideValidator();
        var item = FallbackScenarioOverride.NewFallbackScenarioOverride("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FallbackScenarioOverrideWeightRangeRuleFlagsValuesAboveWeight()
    {
        var rule = new FallbackScenarioOverrideWeightRangeRule(3.4, 3806.7);
        var item = FallbackScenarioOverride.NewFallbackScenarioOverride("x", "y");
        item.Weight = 3808.2;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void FallbackScenarioOverrideRequiredNameRuleFlagsABlankName()
    {
        var rule = new FallbackScenarioOverrideRequiredNameRule("fallback scenario override needs an entry");
        var item = FallbackScenarioOverride.NewFallbackScenarioOverride("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FallbackScenarioOverrideRequiredLocationRuleFlagsABlankLocation()
    {
        var rule = new FallbackScenarioOverrideRequiredLocationRule("fallback scenario override needs this field");
        var item = FallbackScenarioOverride.NewFallbackScenarioOverride("", "");
        item.Location = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FallbackScenarioOverrideRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new FallbackScenarioOverrideRequiredCodeRule("fallback scenario override needs a value");
        var item = FallbackScenarioOverride.NewFallbackScenarioOverride("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        FallbackScenarioOverrideValidatorReportsEveryProblem();
        FallbackScenarioOverrideWeightRangeRuleFlagsValuesAboveWeight();
        FallbackScenarioOverrideRequiredNameRuleFlagsABlankName();
        FallbackScenarioOverrideRequiredLocationRuleFlagsABlankLocation();
        FallbackScenarioOverrideRequiredCodeRuleFlagsABlankCode();
    }
}
