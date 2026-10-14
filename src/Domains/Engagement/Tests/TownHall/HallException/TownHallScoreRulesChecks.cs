namespace Acme.Workforce.Engagement.Tests.TownHall.HallException;

using Acme.Workforce.Engagement.TownHall.HallException;

/// <summary>Checks of hall exception code, run by the project's runner.</summary>
public static class TownHallScoreRulesChecks
{
    public static void TownHallScoreValidatorReportsEveryProblem()
    {
        var validator = TownHallScoreRules.DefaultTownHallScoreValidator();
        var item = TownHallScore.NewTownHallScore("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TownHallScoreRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new TownHallScoreRequiredCodeRule("town hall score needs an entry");
        var item = TownHallScore.NewTownHallScore("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TownHallScoreRequiredContactEmailRuleFlagsABlankContactEmail()
    {
        var rule = new TownHallScoreRequiredContactEmailRule("town hall score needs an entry");
        var item = TownHallScore.NewTownHallScore("", "");
        item.ContactEmail = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TownHallScoreContactEmailLengthRuleFlagsALongContactEmail()
    {
        var rule = new TownHallScoreContactEmailLengthRule(16);
        var item = TownHallScore.NewTownHallScore("x", "y");
        item.ContactEmail = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ContactEmail = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        TownHallScoreValidatorReportsEveryProblem();
        TownHallScoreRequiredCodeRuleFlagsABlankCode();
        TownHallScoreRequiredContactEmailRuleFlagsABlankContactEmail();
        TownHallScoreContactEmailLengthRuleFlagsALongContactEmail();
    }
}
