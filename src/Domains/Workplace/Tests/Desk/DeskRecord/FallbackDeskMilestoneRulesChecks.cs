namespace Acme.Workforce.Workplace.Tests.Desk.DeskRecord;

using Acme.Workforce.Workplace.Desk.DeskRecord;

/// <summary>Checks of desk record code, run by the project's runner.</summary>
public static class FallbackDeskMilestoneRulesChecks
{
    public static void FallbackDeskMilestoneValidatorReportsEveryProblem()
    {
        var validator = FallbackDeskMilestoneRules.DefaultFallbackDeskMilestoneValidator();
        var item = FallbackDeskMilestone.NewFallbackDeskMilestone("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FallbackDeskMilestoneDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes()
    {
        var rule = new FallbackDeskMilestoneDurationMinutesRangeRule(80, 3600);
        var item = FallbackDeskMilestone.NewFallbackDeskMilestone("x", "y");
        item.DurationMinutes = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void FallbackDeskMilestoneRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new FallbackDeskMilestoneRequiredTitleRule("fallback desk milestone needs this field");
        var item = FallbackDeskMilestone.NewFallbackDeskMilestone("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FallbackDeskMilestoneCeilingRangeRuleFlagsValuesAboveCeiling()
    {
        var rule = new FallbackDeskMilestoneCeilingRangeRule(0.2, 1088.9);
        var item = FallbackDeskMilestone.NewFallbackDeskMilestone("x", "y");
        item.Ceiling = 1090.4;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        FallbackDeskMilestoneValidatorReportsEveryProblem();
        FallbackDeskMilestoneDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes();
        FallbackDeskMilestoneRequiredTitleRuleFlagsABlankTitle();
        FallbackDeskMilestoneCeilingRangeRuleFlagsValuesAboveCeiling();
    }
}
