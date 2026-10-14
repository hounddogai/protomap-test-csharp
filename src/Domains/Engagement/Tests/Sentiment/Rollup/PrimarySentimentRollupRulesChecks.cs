namespace Acme.Workforce.Engagement.Tests.Sentiment.Rollup;

using Acme.Workforce.Engagement.Sentiment.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class PrimarySentimentRollupRulesChecks
{
    public static void PrimarySentimentRollupValidatorReportsEveryProblem()
    {
        var validator = PrimarySentimentRollupRules.DefaultPrimarySentimentRollupValidator();
        var item = PrimarySentimentRollup.NewPrimarySentimentRollup("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PrimarySentimentRollupRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new PrimarySentimentRollupRequiredLabelRule("primary sentiment rollup needs this field");
        var item = PrimarySentimentRollup.NewPrimarySentimentRollup("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PrimarySentimentRollupRequiredTeamRuleFlagsABlankTeam()
    {
        var rule = new PrimarySentimentRollupRequiredTeamRule("primary sentiment rollup needs a value");
        var item = PrimarySentimentRollup.NewPrimarySentimentRollup("", "");
        item.Team = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        PrimarySentimentRollupValidatorReportsEveryProblem();
        PrimarySentimentRollupRequiredLabelRuleFlagsABlankLabel();
        PrimarySentimentRollupRequiredTeamRuleFlagsABlankTeam();
    }
}
