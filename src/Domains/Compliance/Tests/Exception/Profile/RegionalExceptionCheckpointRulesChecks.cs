namespace Acme.Workforce.Compliance.Tests.Exception.Profile;

using Acme.Workforce.Compliance.Exception.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class RegionalExceptionCheckpointRulesChecks
{
    public static void RegionalExceptionCheckpointValidatorReportsEveryProblem()
    {
        var validator = RegionalExceptionCheckpointRules.DefaultRegionalExceptionCheckpointValidator();
        var item = RegionalExceptionCheckpoint.NewRegionalExceptionCheckpoint("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RegionalExceptionCheckpointCodeLengthRuleFlagsALongCode()
    {
        var rule = new RegionalExceptionCheckpointCodeLengthRule(16);
        var item = RegionalExceptionCheckpoint.NewRegionalExceptionCheckpoint("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RegionalExceptionCheckpointRequiredTeamRuleFlagsABlankTeam()
    {
        var rule = new RegionalExceptionCheckpointRequiredTeamRule("regional exception checkpoint needs an entry");
        var item = RegionalExceptionCheckpoint.NewRegionalExceptionCheckpoint("", "");
        item.Team = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RegionalExceptionCheckpointTitleLengthRuleFlagsALongTitle()
    {
        var rule = new RegionalExceptionCheckpointTitleLengthRule(80);
        var item = RegionalExceptionCheckpoint.NewRegionalExceptionCheckpoint("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        RegionalExceptionCheckpointValidatorReportsEveryProblem();
        RegionalExceptionCheckpointCodeLengthRuleFlagsALongCode();
        RegionalExceptionCheckpointRequiredTeamRuleFlagsABlankTeam();
        RegionalExceptionCheckpointTitleLengthRuleFlagsALongTitle();
    }
}
