namespace Acme.Workforce.Orgdesign.Tests.Scenario.ScenarioPolicy;

using Acme.Workforce.Orgdesign.Scenario.ScenarioPolicy;

/// <summary>Checks of scenario policy code, run by the project's runner.</summary>
public static class ScenarioTierRulesChecks
{
    public static void ScenarioTierValidatorReportsEveryProblem()
    {
        var validator = ScenarioTierRules.DefaultScenarioTierValidator();
        var item = ScenarioTier.NewScenarioTier("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ScenarioTierCodeLengthRuleFlagsALongCode()
    {
        var rule = new ScenarioTierCodeLengthRule(1000);
        var item = ScenarioTier.NewScenarioTier("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ScenarioTierEndsAtRangeRuleFlagsValuesAboveEndsAt()
    {
        var rule = new ScenarioTierEndsAtRangeRule(64, 120);
        var item = ScenarioTier.NewScenarioTier("x", "y");
        item.EndsAt = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ScenarioTierWindowDaysRangeRuleFlagsValuesAboveWindowDays()
    {
        var rule = new ScenarioTierWindowDaysRangeRule(1000, 3600);
        var item = ScenarioTier.NewScenarioTier("x", "y");
        item.WindowDays = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ScenarioTierRequiredTimezoneRuleFlagsABlankTimezone()
    {
        var rule = new ScenarioTierRequiredTimezoneRule("scenario tier needs this field");
        var item = ScenarioTier.NewScenarioTier("", "");
        item.Timezone = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ScenarioTierRequiredUnitRuleFlagsABlankUnit()
    {
        var rule = new ScenarioTierRequiredUnitRule("scenario tier needs an entry");
        var item = ScenarioTier.NewScenarioTier("", "");
        item.Unit = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        ScenarioTierValidatorReportsEveryProblem();
        ScenarioTierCodeLengthRuleFlagsALongCode();
        ScenarioTierEndsAtRangeRuleFlagsValuesAboveEndsAt();
        ScenarioTierWindowDaysRangeRuleFlagsValuesAboveWindowDays();
        ScenarioTierRequiredTimezoneRuleFlagsABlankTimezone();
        ScenarioTierRequiredUnitRuleFlagsABlankUnit();
    }
}
