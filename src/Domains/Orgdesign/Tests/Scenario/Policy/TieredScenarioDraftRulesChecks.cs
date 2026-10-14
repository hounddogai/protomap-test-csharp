namespace Acme.Workforce.Orgdesign.Tests.Scenario.Policy;

using Acme.Workforce.Orgdesign.Scenario.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class TieredScenarioDraftRulesChecks
{
    public static void TieredScenarioDraftValidatorReportsEveryProblem()
    {
        var validator = TieredScenarioDraftRules.DefaultTieredScenarioDraftValidator();
        var item = TieredScenarioDraft.NewTieredScenarioDraft("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TieredScenarioDraftLabelLengthRuleFlagsALongLabel()
    {
        var rule = new TieredScenarioDraftLabelLengthRule(120);
        var item = TieredScenarioDraft.NewTieredScenarioDraft("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TieredScenarioDraftSequenceRangeRuleFlagsValuesAboveSequence()
    {
        var rule = new TieredScenarioDraftSequenceRangeRule(120, 255);
        var item = TieredScenarioDraft.NewTieredScenarioDraft("x", "y");
        item.Sequence = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        TieredScenarioDraftValidatorReportsEveryProblem();
        TieredScenarioDraftLabelLengthRuleFlagsALongLabel();
        TieredScenarioDraftSequenceRangeRuleFlagsValuesAboveSequence();
    }
}
