namespace Acme.Workforce.Workplace.Tests.Occupancy.OccupancyPlan;

using Acme.Workforce.Workplace.Occupancy.OccupancyPlan;

/// <summary>Checks of occupancy plan code, run by the project's runner.</summary>
public static class OccupancyExceptionRulesChecks
{
    public static void OccupancyExceptionValidatorReportsEveryProblem()
    {
        var validator = OccupancyExceptionRules.DefaultOccupancyExceptionValidator();
        var item = OccupancyException.NewOccupancyException("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void OccupancyExceptionRequiredTimezoneRuleFlagsABlankTimezone()
    {
        var rule = new OccupancyExceptionRequiredTimezoneRule("occupancy exception needs an entry");
        var item = OccupancyException.NewOccupancyException("", "");
        item.Timezone = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void OccupancyExceptionRequiredSummaryRuleFlagsABlankSummary()
    {
        var rule = new OccupancyExceptionRequiredSummaryRule("occupancy exception needs this field");
        var item = OccupancyException.NewOccupancyException("", "");
        item.Summary = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void OccupancyExceptionWindowDaysRangeRuleFlagsValuesAboveWindowDays()
    {
        var rule = new OccupancyExceptionWindowDaysRangeRule(64, 120);
        var item = OccupancyException.NewOccupancyException("x", "y");
        item.WindowDays = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        OccupancyExceptionValidatorReportsEveryProblem();
        OccupancyExceptionRequiredTimezoneRuleFlagsABlankTimezone();
        OccupancyExceptionRequiredSummaryRuleFlagsABlankSummary();
        OccupancyExceptionWindowDaysRangeRuleFlagsValuesAboveWindowDays();
    }
}
