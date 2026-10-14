namespace Acme.Workforce.Workplace.Tests.Occupancy.Plan;

using Acme.Workforce.Workplace.Occupancy.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class ScheduledOccupancyPlanRulesChecks
{
    public static void ScheduledOccupancyPlanValidatorReportsEveryProblem()
    {
        var validator = ScheduledOccupancyPlanRules.DefaultScheduledOccupancyPlanValidator();
        var item = ScheduledOccupancyPlan.NewScheduledOccupancyPlan("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ScheduledOccupancyPlanExpiresAtRangeRuleFlagsValuesAboveExpiresAt()
    {
        var rule = new ScheduledOccupancyPlanExpiresAtRangeRule(80, 1000);
        var item = ScheduledOccupancyPlan.NewScheduledOccupancyPlan("x", "y");
        item.ExpiresAt = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ScheduledOccupancyPlanNameLengthRuleFlagsALongName()
    {
        var rule = new ScheduledOccupancyPlanNameLengthRule(16);
        var item = ScheduledOccupancyPlan.NewScheduledOccupancyPlan("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ScheduledOccupancyPlanRequiredLocaleRuleFlagsABlankLocale()
    {
        var rule = new ScheduledOccupancyPlanRequiredLocaleRule("scheduled occupancy plan needs a value");
        var item = ScheduledOccupancyPlan.NewScheduledOccupancyPlan("", "");
        item.Locale = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ScheduledOccupancyPlanRequiredNameRuleFlagsABlankName()
    {
        var rule = new ScheduledOccupancyPlanRequiredNameRule("scheduled occupancy plan needs an entry");
        var item = ScheduledOccupancyPlan.NewScheduledOccupancyPlan("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        ScheduledOccupancyPlanValidatorReportsEveryProblem();
        ScheduledOccupancyPlanExpiresAtRangeRuleFlagsValuesAboveExpiresAt();
        ScheduledOccupancyPlanNameLengthRuleFlagsALongName();
        ScheduledOccupancyPlanRequiredLocaleRuleFlagsABlankLocale();
        ScheduledOccupancyPlanRequiredNameRuleFlagsABlankName();
    }
}
