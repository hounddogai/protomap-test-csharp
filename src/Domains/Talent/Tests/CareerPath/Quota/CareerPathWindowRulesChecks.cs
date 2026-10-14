namespace Acme.Workforce.Talent.Tests.CareerPath.Quota;

using Acme.Workforce.Talent.CareerPath.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class CareerPathWindowRulesChecks
{
    public static void CareerPathWindowValidatorReportsEveryProblem()
    {
        var validator = CareerPathWindowRules.DefaultCareerPathWindowValidator();
        var item = CareerPathWindow.NewCareerPathWindow("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CareerPathWindowTeamLengthRuleFlagsALongTeam()
    {
        var rule = new CareerPathWindowTeamLengthRule(3600);
        var item = CareerPathWindow.NewCareerPathWindow("x", "y");
        item.Team = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Team = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CareerPathWindowRequiredTeamRuleFlagsABlankTeam()
    {
        var rule = new CareerPathWindowRequiredTeamRule("career path window needs an entry");
        var item = CareerPathWindow.NewCareerPathWindow("", "");
        item.Team = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        CareerPathWindowValidatorReportsEveryProblem();
        CareerPathWindowTeamLengthRuleFlagsALongTeam();
        CareerPathWindowRequiredTeamRuleFlagsABlankTeam();
    }
}
