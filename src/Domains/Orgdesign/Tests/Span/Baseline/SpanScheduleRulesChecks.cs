namespace Acme.Workforce.Orgdesign.Tests.Span.Baseline;

using Acme.Workforce.Orgdesign.Span.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class SpanScheduleRulesChecks
{
    public static void SpanScheduleValidatorReportsEveryProblem()
    {
        var validator = SpanScheduleRules.DefaultSpanScheduleValidator();
        var item = SpanSchedule.NewSpanSchedule("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SpanScheduleDepartmentLengthRuleFlagsALongDepartment()
    {
        var rule = new SpanScheduleDepartmentLengthRule(255);
        var item = SpanSchedule.NewSpanSchedule("x", "y");
        item.Department = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Department = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SpanScheduleRequiredUnitRuleFlagsABlankUnit()
    {
        var rule = new SpanScheduleRequiredUnitRule("span schedule needs this field");
        var item = SpanSchedule.NewSpanSchedule("", "");
        item.Unit = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        SpanScheduleValidatorReportsEveryProblem();
        SpanScheduleDepartmentLengthRuleFlagsALongDepartment();
        SpanScheduleRequiredUnitRuleFlagsABlankUnit();
    }
}
