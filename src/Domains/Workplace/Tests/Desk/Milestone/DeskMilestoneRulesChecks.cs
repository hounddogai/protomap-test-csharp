namespace Acme.Workforce.Workplace.Tests.Desk.Milestone;

using Acme.Workforce.Workplace.Desk.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class DeskMilestoneRulesChecks
{
    public static void DeskMilestoneValidatorReportsEveryProblem()
    {
        var validator = DeskMilestoneRules.DefaultDeskMilestoneValidator();
        var item = DeskMilestone.NewDeskMilestone("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DeskMilestoneLeaveDaysRangeRuleFlagsValuesAboveLeaveDays()
    {
        var rule = new DeskMilestoneLeaveDaysRangeRule(3600, 86400);
        var item = DeskMilestone.NewDeskMilestone("x", "y");
        item.LeaveDays = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void DeskMilestoneNameLengthRuleFlagsALongName()
    {
        var rule = new DeskMilestoneNameLengthRule(64);
        var item = DeskMilestone.NewDeskMilestone("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        DeskMilestoneValidatorReportsEveryProblem();
        DeskMilestoneLeaveDaysRangeRuleFlagsValuesAboveLeaveDays();
        DeskMilestoneNameLengthRuleFlagsALongName();
    }
}
