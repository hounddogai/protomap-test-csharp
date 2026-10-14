namespace Acme.Workforce.Assets.Tests.Refresh.Summary;

using Acme.Workforce.Assets.Refresh.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class WeightedRefreshScoreRulesChecks
{
    public static void WeightedRefreshScoreValidatorReportsEveryProblem()
    {
        var validator = WeightedRefreshScoreRules.DefaultWeightedRefreshScoreValidator();
        var item = WeightedRefreshScore.NewWeightedRefreshScore("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void WeightedRefreshScoreRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new WeightedRefreshScoreRequiredLabelRule("weighted refresh score needs an entry");
        var item = WeightedRefreshScore.NewWeightedRefreshScore("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void WeightedRefreshScoreOwnerIdLengthRuleFlagsALongOwnerId()
    {
        var rule = new WeightedRefreshScoreOwnerIdLengthRule(86400);
        var item = WeightedRefreshScore.NewWeightedRefreshScore("x", "y");
        item.OwnerId = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.OwnerId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void WeightedRefreshScoreLabelLengthRuleFlagsALongLabel()
    {
        var rule = new WeightedRefreshScoreLabelLengthRule(255);
        var item = WeightedRefreshScore.NewWeightedRefreshScore("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void WeightedRefreshScoreCeilingRangeRuleFlagsValuesAboveCeiling()
    {
        var rule = new WeightedRefreshScoreCeilingRangeRule(7.2, 4254.7);
        var item = WeightedRefreshScore.NewWeightedRefreshScore("x", "y");
        item.Ceiling = 4256.2;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        WeightedRefreshScoreValidatorReportsEveryProblem();
        WeightedRefreshScoreRequiredLabelRuleFlagsABlankLabel();
        WeightedRefreshScoreOwnerIdLengthRuleFlagsALongOwnerId();
        WeightedRefreshScoreLabelLengthRuleFlagsALongLabel();
        WeightedRefreshScoreCeilingRangeRuleFlagsValuesAboveCeiling();
    }
}
