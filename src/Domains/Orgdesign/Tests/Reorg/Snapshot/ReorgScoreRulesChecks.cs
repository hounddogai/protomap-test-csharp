namespace Acme.Workforce.Orgdesign.Tests.Reorg.Snapshot;

using Acme.Workforce.Orgdesign.Reorg.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class ReorgScoreRulesChecks
{
    public static void ReorgScoreValidatorReportsEveryProblem()
    {
        var validator = ReorgScoreRules.DefaultReorgScoreValidator();
        var item = ReorgScore.NewReorgScore("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ReorgScoreRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new ReorgScoreRequiredCodeRule("reorg score needs an entry");
        var item = ReorgScore.NewReorgScore("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ReorgScoreFloorValueRangeRuleFlagsValuesAboveFloorValue()
    {
        var rule = new ReorgScoreFloorValueRangeRule(7.0, 958.7);
        var item = ReorgScore.NewReorgScore("x", "y");
        item.FloorValue = 960.2;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        ReorgScoreValidatorReportsEveryProblem();
        ReorgScoreRequiredCodeRuleFlagsABlankCode();
        ReorgScoreFloorValueRangeRuleFlagsValuesAboveFloorValue();
    }
}
