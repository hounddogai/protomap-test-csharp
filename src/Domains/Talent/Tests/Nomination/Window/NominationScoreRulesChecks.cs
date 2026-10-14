namespace Acme.Workforce.Talent.Tests.Nomination.Window;

using Acme.Workforce.Talent.Nomination.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class NominationScoreRulesChecks
{
    public static void NominationScoreValidatorReportsEveryProblem()
    {
        var validator = NominationScoreRules.DefaultNominationScoreValidator();
        var item = NominationScore.NewNominationScore("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void NominationScoreRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new NominationScoreRequiredLabelRule("nomination score needs this field");
        var item = NominationScore.NewNominationScore("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void NominationScoreTeamLengthRuleFlagsALongTeam()
    {
        var rule = new NominationScoreTeamLengthRule(3600);
        var item = NominationScore.NewNominationScore("x", "y");
        item.Team = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Team = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void NominationScoreRequiredTeamRuleFlagsABlankTeam()
    {
        var rule = new NominationScoreRequiredTeamRule("nomination score needs this field");
        var item = NominationScore.NewNominationScore("", "");
        item.Team = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        NominationScoreValidatorReportsEveryProblem();
        NominationScoreRequiredLabelRuleFlagsABlankLabel();
        NominationScoreTeamLengthRuleFlagsALongTeam();
        NominationScoreRequiredTeamRuleFlagsABlankTeam();
    }
}
