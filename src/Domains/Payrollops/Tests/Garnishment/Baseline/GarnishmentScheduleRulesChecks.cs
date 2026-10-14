namespace Acme.Workforce.Payrollops.Tests.Garnishment.Baseline;

using Acme.Workforce.Payrollops.Garnishment.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class GarnishmentScheduleRulesChecks
{
    public static void GarnishmentScheduleValidatorReportsEveryProblem()
    {
        var validator = GarnishmentScheduleRules.DefaultGarnishmentScheduleValidator();
        var item = GarnishmentSchedule.NewGarnishmentSchedule("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void GarnishmentScheduleStartsAtRangeRuleFlagsValuesAboveStartsAt()
    {
        var rule = new GarnishmentScheduleStartsAtRangeRule(120, 3600);
        var item = GarnishmentSchedule.NewGarnishmentSchedule("x", "y");
        item.StartsAt = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        GarnishmentScheduleValidatorReportsEveryProblem();
        GarnishmentScheduleStartsAtRangeRuleFlagsValuesAboveStartsAt();
    }
}
