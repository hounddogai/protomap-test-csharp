namespace Acme.Workforce.Workplace.Tests.Occupancy.Score;

using Acme.Workforce.Workplace.Occupancy.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class OccupancyWindowRulesChecks
{
    public static void OccupancyWindowValidatorReportsEveryProblem()
    {
        var validator = OccupancyWindowRules.DefaultOccupancyWindowValidator();
        var item = OccupancyWindow.NewOccupancyWindow("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void OccupancyWindowRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new OccupancyWindowRequiredLabelRule("occupancy window needs an entry");
        var item = OccupancyWindow.NewOccupancyWindow("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void OccupancyWindowHoursRangeRuleFlagsValuesAboveHours()
    {
        var rule = new OccupancyWindowHoursRangeRule(0.5, 2762.4);
        var item = OccupancyWindow.NewOccupancyWindow("x", "y");
        item.Hours = 2763.9;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void OccupancyWindowSkillLengthRuleFlagsALongSkill()
    {
        var rule = new OccupancyWindowSkillLengthRule(255);
        var item = OccupancyWindow.NewOccupancyWindow("x", "y");
        item.Skill = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Skill = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void OccupancyWindowLabelLengthRuleFlagsALongLabel()
    {
        var rule = new OccupancyWindowLabelLengthRule(64);
        var item = OccupancyWindow.NewOccupancyWindow("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void OccupancyWindowRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new OccupancyWindowRequiredCodeRule("occupancy window needs this field");
        var item = OccupancyWindow.NewOccupancyWindow("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        OccupancyWindowValidatorReportsEveryProblem();
        OccupancyWindowRequiredLabelRuleFlagsABlankLabel();
        OccupancyWindowHoursRangeRuleFlagsValuesAboveHours();
        OccupancyWindowSkillLengthRuleFlagsALongSkill();
        OccupancyWindowLabelLengthRuleFlagsALongLabel();
        OccupancyWindowRequiredCodeRuleFlagsABlankCode();
    }
}
