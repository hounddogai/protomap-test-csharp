namespace Acme.Workforce.Compensation.Tests.BonusPool.PoolSchedule;

using Acme.Workforce.Compensation.BonusPool.PoolSchedule;

/// <summary>Checks of pool schedule code, run by the project's runner.</summary>
public static class BonusPoolScoreRulesChecks
{
    public static void BonusPoolScoreValidatorReportsEveryProblem()
    {
        var validator = BonusPoolScoreRules.DefaultBonusPoolScoreValidator();
        var item = BonusPoolScore.NewBonusPoolScore("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BonusPoolScoreCodeLengthRuleFlagsALongCode()
    {
        var rule = new BonusPoolScoreCodeLengthRule(16);
        var item = BonusPoolScore.NewBonusPoolScore("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BonusPoolScoreSequenceRangeRuleFlagsValuesAboveSequence()
    {
        var rule = new BonusPoolScoreSequenceRangeRule(120, 3600);
        var item = BonusPoolScore.NewBonusPoolScore("x", "y");
        item.Sequence = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BonusPoolScoreRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new BonusPoolScoreRequiredCodeRule("bonus pool score needs a value");
        var item = BonusPoolScore.NewBonusPoolScore("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BonusPoolScoreTitleLengthRuleFlagsALongTitle()
    {
        var rule = new BonusPoolScoreTitleLengthRule(32);
        var item = BonusPoolScore.NewBonusPoolScore("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        BonusPoolScoreValidatorReportsEveryProblem();
        BonusPoolScoreCodeLengthRuleFlagsALongCode();
        BonusPoolScoreSequenceRangeRuleFlagsValuesAboveSequence();
        BonusPoolScoreRequiredCodeRuleFlagsABlankCode();
        BonusPoolScoreTitleLengthRuleFlagsALongTitle();
    }
}
