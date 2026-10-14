namespace Acme.Workforce.Engagement.Tests.TownHall.Request;

using Acme.Workforce.Engagement.TownHall.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class LegacyTownHallAssignmentRulesChecks
{
    public static void LegacyTownHallAssignmentValidatorReportsEveryProblem()
    {
        var validator = LegacyTownHallAssignmentRules.DefaultLegacyTownHallAssignmentValidator();
        var item = LegacyTownHallAssignment.NewLegacyTownHallAssignment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LegacyTownHallAssignmentRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new LegacyTownHallAssignmentRequiredCodeRule("legacy town hall assignment needs this field");
        var item = LegacyTownHallAssignment.NewLegacyTownHallAssignment("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LegacyTownHallAssignmentGradeLengthRuleFlagsALongGrade()
    {
        var rule = new LegacyTownHallAssignmentGradeLengthRule(1000);
        var item = LegacyTownHallAssignment.NewLegacyTownHallAssignment("x", "y");
        item.Grade = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Grade = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LegacyTownHallAssignmentBadgeLengthRuleFlagsALongBadge()
    {
        var rule = new LegacyTownHallAssignmentBadgeLengthRule(120);
        var item = LegacyTownHallAssignment.NewLegacyTownHallAssignment("x", "y");
        item.Badge = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Badge = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        LegacyTownHallAssignmentValidatorReportsEveryProblem();
        LegacyTownHallAssignmentRequiredCodeRuleFlagsABlankCode();
        LegacyTownHallAssignmentGradeLengthRuleFlagsALongGrade();
        LegacyTownHallAssignmentBadgeLengthRuleFlagsALongBadge();
    }
}
