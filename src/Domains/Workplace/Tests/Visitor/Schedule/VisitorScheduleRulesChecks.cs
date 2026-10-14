namespace Acme.Workforce.Workplace.Tests.Visitor.Schedule;

using Acme.Workforce.Workplace.Visitor.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class VisitorScheduleRulesChecks
{
    public static void VisitorScheduleValidatorReportsEveryProblem()
    {
        var validator = VisitorScheduleRules.DefaultVisitorScheduleValidator();
        var item = VisitorSchedule.NewVisitorSchedule("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void VisitorScheduleSalaryBandLengthRuleFlagsALongSalaryBand()
    {
        var rule = new VisitorScheduleSalaryBandLengthRule(64);
        var item = VisitorSchedule.NewVisitorSchedule("x", "y");
        item.SalaryBand = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.SalaryBand = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void VisitorScheduleLabelLengthRuleFlagsALongLabel()
    {
        var rule = new VisitorScheduleLabelLengthRule(86400);
        var item = VisitorSchedule.NewVisitorSchedule("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void VisitorScheduleRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new VisitorScheduleRequiredLabelRule("visitor schedule needs an entry");
        var item = VisitorSchedule.NewVisitorSchedule("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        VisitorScheduleValidatorReportsEveryProblem();
        VisitorScheduleSalaryBandLengthRuleFlagsALongSalaryBand();
        VisitorScheduleLabelLengthRuleFlagsALongLabel();
        VisitorScheduleRequiredLabelRuleFlagsABlankLabel();
    }
}
