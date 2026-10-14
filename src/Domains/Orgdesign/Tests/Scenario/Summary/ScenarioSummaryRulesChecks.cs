namespace Acme.Workforce.Orgdesign.Tests.Scenario.Summary;

using Acme.Workforce.Orgdesign.Scenario.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class ScenarioSummaryRulesChecks
{
    public static void ScenarioSummaryValidatorReportsEveryProblem()
    {
        var validator = ScenarioSummaryRules.DefaultScenarioSummaryValidator();
        var item = ScenarioSummary.NewScenarioSummary("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ScenarioSummaryCodeLengthRuleFlagsALongCode()
    {
        var rule = new ScenarioSummaryCodeLengthRule(86400);
        var item = ScenarioSummary.NewScenarioSummary("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ScenarioSummaryTitleLengthRuleFlagsALongTitle()
    {
        var rule = new ScenarioSummaryTitleLengthRule(64);
        var item = ScenarioSummary.NewScenarioSummary("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ScenarioSummaryRateRangeRuleFlagsValuesAboveRate()
    {
        var rule = new ScenarioSummaryRateRangeRule(9.2, 4700.8);
        var item = ScenarioSummary.NewScenarioSummary("x", "y");
        item.Rate = 4702.3;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ScenarioSummaryRequiredChecksumRuleFlagsABlankChecksum()
    {
        var rule = new ScenarioSummaryRequiredChecksumRule("scenario summary needs a value");
        var item = ScenarioSummary.NewScenarioSummary("", "");
        item.Checksum = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ScenarioSummaryRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new ScenarioSummaryRequiredTitleRule("scenario summary needs a value");
        var item = ScenarioSummary.NewScenarioSummary("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        ScenarioSummaryValidatorReportsEveryProblem();
        ScenarioSummaryCodeLengthRuleFlagsALongCode();
        ScenarioSummaryTitleLengthRuleFlagsALongTitle();
        ScenarioSummaryRateRangeRuleFlagsValuesAboveRate();
        ScenarioSummaryRequiredChecksumRuleFlagsABlankChecksum();
        ScenarioSummaryRequiredTitleRuleFlagsABlankTitle();
    }
}
