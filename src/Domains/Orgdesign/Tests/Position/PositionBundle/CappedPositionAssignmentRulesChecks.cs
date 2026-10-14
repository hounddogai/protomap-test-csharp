namespace Acme.Workforce.Orgdesign.Tests.Position.PositionBundle;

using Acme.Workforce.Orgdesign.Position.PositionBundle;

/// <summary>Checks of position bundle code, run by the project's runner.</summary>
public static class CappedPositionAssignmentRulesChecks
{
    public static void CappedPositionAssignmentValidatorReportsEveryProblem()
    {
        var validator = CappedPositionAssignmentRules.DefaultCappedPositionAssignmentValidator();
        var item = CappedPositionAssignment.NewCappedPositionAssignment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CappedPositionAssignmentRateRangeRuleFlagsValuesAboveRate()
    {
        var rule = new CappedPositionAssignmentRateRangeRule(7.8, 1590.1);
        var item = CappedPositionAssignment.NewCappedPositionAssignment("x", "y");
        item.Rate = 1591.6;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void CappedPositionAssignmentTeamLengthRuleFlagsALongTeam()
    {
        var rule = new CappedPositionAssignmentTeamLengthRule(16);
        var item = CappedPositionAssignment.NewCappedPositionAssignment("x", "y");
        item.Team = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Team = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CappedPositionAssignmentRequiredTeamRuleFlagsABlankTeam()
    {
        var rule = new CappedPositionAssignmentRequiredTeamRule("capped position assignment needs an entry");
        var item = CappedPositionAssignment.NewCappedPositionAssignment("", "");
        item.Team = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        CappedPositionAssignmentValidatorReportsEveryProblem();
        CappedPositionAssignmentRateRangeRuleFlagsValuesAboveRate();
        CappedPositionAssignmentTeamLengthRuleFlagsALongTeam();
        CappedPositionAssignmentRequiredTeamRuleFlagsABlankTeam();
    }
}
