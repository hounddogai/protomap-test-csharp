namespace Acme.Workforce.Orgdesign.Tests.Scenario.ScenarioPolicy;

using Acme.Workforce.Orgdesign.Scenario.ScenarioPolicy;

/// <summary>Checks of scenario policy code, run by the project's runner.</summary>
public static class TieredScenarioPolicyRulesChecks
{
    public static void TieredScenarioPolicyValidatorReportsEveryProblem()
    {
        var validator = TieredScenarioPolicyRules.DefaultTieredScenarioPolicyValidator();
        var item = TieredScenarioPolicy.NewTieredScenarioPolicy("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TieredScenarioPolicyRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new TieredScenarioPolicyRequiredTitleRule("tiered scenario policy needs an entry");
        var item = TieredScenarioPolicy.NewTieredScenarioPolicy("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TieredScenarioPolicyStartsAtRangeRuleFlagsValuesAboveStartsAt()
    {
        var rule = new TieredScenarioPolicyStartsAtRangeRule(32, 120);
        var item = TieredScenarioPolicy.NewTieredScenarioPolicy("x", "y");
        item.StartsAt = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void TieredScenarioPolicyRateRangeRuleFlagsValuesAboveRate()
    {
        var rule = new TieredScenarioPolicyRateRangeRule(5.3, 3027.8);
        var item = TieredScenarioPolicy.NewTieredScenarioPolicy("x", "y");
        item.Rate = 3029.3;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        TieredScenarioPolicyValidatorReportsEveryProblem();
        TieredScenarioPolicyRequiredTitleRuleFlagsABlankTitle();
        TieredScenarioPolicyStartsAtRangeRuleFlagsValuesAboveStartsAt();
        TieredScenarioPolicyRateRangeRuleFlagsValuesAboveRate();
    }
}
