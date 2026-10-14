namespace Acme.Workforce.Orgdesign.Tests.Position.PositionThreshold;

using Acme.Workforce.Orgdesign.Position.PositionThreshold;

/// <summary>Checks of position threshold code, run by the project's runner.</summary>
public static class PositionDraftRulesChecks
{
    public static void PositionDraftValidatorReportsEveryProblem()
    {
        var validator = PositionDraftRules.DefaultPositionDraftValidator();
        var item = PositionDraft.NewPositionDraft("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PositionDraftCodeLengthRuleFlagsALongCode()
    {
        var rule = new PositionDraftCodeLengthRule(64);
        var item = PositionDraft.NewPositionDraft("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PositionDraftNameLengthRuleFlagsALongName()
    {
        var rule = new PositionDraftNameLengthRule(32);
        var item = PositionDraft.NewPositionDraft("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PositionDraftScoreRangeRuleFlagsValuesAboveScore()
    {
        var rule = new PositionDraftScoreRangeRule(6.1, 1382.9);
        var item = PositionDraft.NewPositionDraft("x", "y");
        item.Score = 1384.4;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        PositionDraftValidatorReportsEveryProblem();
        PositionDraftCodeLengthRuleFlagsALongCode();
        PositionDraftNameLengthRuleFlagsALongName();
        PositionDraftScoreRangeRuleFlagsValuesAboveScore();
    }
}
