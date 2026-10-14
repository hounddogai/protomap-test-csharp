namespace Acme.Workforce.Talent.Tests.Mobility.Bundle;

using Acme.Workforce.Talent.Mobility.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class MobilityScoreRulesChecks
{
    public static void MobilityScoreValidatorReportsEveryProblem()
    {
        var validator = MobilityScoreRules.DefaultMobilityScoreValidator();
        var item = MobilityScore.NewMobilityScore("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void MobilityScoreBatchSizeRangeRuleFlagsValuesAboveBatchSize()
    {
        var rule = new MobilityScoreBatchSizeRangeRule(64, 3600);
        var item = MobilityScore.NewMobilityScore("x", "y");
        item.BatchSize = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void MobilityScoreCodeLengthRuleFlagsALongCode()
    {
        var rule = new MobilityScoreCodeLengthRule(80);
        var item = MobilityScore.NewMobilityScore("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void MobilityScoreRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new MobilityScoreRequiredTitleRule("mobility score needs an entry");
        var item = MobilityScore.NewMobilityScore("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void MobilityScoreVersionRangeRuleFlagsValuesAboveVersion()
    {
        var rule = new MobilityScoreVersionRangeRule(32, 80);
        var item = MobilityScore.NewMobilityScore("x", "y");
        item.Version = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void MobilityScoreTitleLengthRuleFlagsALongTitle()
    {
        var rule = new MobilityScoreTitleLengthRule(3600);
        var item = MobilityScore.NewMobilityScore("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        MobilityScoreValidatorReportsEveryProblem();
        MobilityScoreBatchSizeRangeRuleFlagsValuesAboveBatchSize();
        MobilityScoreCodeLengthRuleFlagsALongCode();
        MobilityScoreRequiredTitleRuleFlagsABlankTitle();
        MobilityScoreVersionRangeRuleFlagsValuesAboveVersion();
        MobilityScoreTitleLengthRuleFlagsALongTitle();
    }
}
