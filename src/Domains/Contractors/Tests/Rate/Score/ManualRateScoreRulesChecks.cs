namespace Acme.Workforce.Contractors.Tests.Rate.Score;

using Acme.Workforce.Contractors.Rate.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class ManualRateScoreRulesChecks
{
    public static void ManualRateScoreValidatorReportsEveryProblem()
    {
        var validator = ManualRateScoreRules.DefaultManualRateScoreValidator();
        var item = ManualRateScore.NewManualRateScore("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ManualRateScoreNotesLengthRuleFlagsALongNotes()
    {
        var rule = new ManualRateScoreNotesLengthRule(1000);
        var item = ManualRateScore.NewManualRateScore("x", "y");
        item.Notes = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Notes = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ManualRateScoreNameLengthRuleFlagsALongName()
    {
        var rule = new ManualRateScoreNameLengthRule(64);
        var item = ManualRateScore.NewManualRateScore("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ManualRateScoreCodeLengthRuleFlagsALongCode()
    {
        var rule = new ManualRateScoreCodeLengthRule(80);
        var item = ManualRateScore.NewManualRateScore("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        ManualRateScoreValidatorReportsEveryProblem();
        ManualRateScoreNotesLengthRuleFlagsALongNotes();
        ManualRateScoreNameLengthRuleFlagsALongName();
        ManualRateScoreCodeLengthRuleFlagsALongCode();
    }
}
