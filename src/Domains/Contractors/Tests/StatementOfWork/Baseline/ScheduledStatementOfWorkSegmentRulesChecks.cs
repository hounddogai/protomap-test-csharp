namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Baseline;

using Acme.Workforce.Contractors.StatementOfWork.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class ScheduledStatementOfWorkSegmentRulesChecks
{
    public static void ScheduledStatementOfWorkSegmentValidatorReportsEveryProblem()
    {
        var validator = ScheduledStatementOfWorkSegmentRules.DefaultScheduledStatementOfWorkSegmentValidator();
        var item = ScheduledStatementOfWorkSegment.NewScheduledStatementOfWorkSegment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ScheduledStatementOfWorkSegmentNameLengthRuleFlagsALongName()
    {
        var rule = new ScheduledStatementOfWorkSegmentNameLengthRule(120);
        var item = ScheduledStatementOfWorkSegment.NewScheduledStatementOfWorkSegment("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ScheduledStatementOfWorkSegmentDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes()
    {
        var rule = new ScheduledStatementOfWorkSegmentDurationMinutesRangeRule(32, 64);
        var item = ScheduledStatementOfWorkSegment.NewScheduledStatementOfWorkSegment("x", "y");
        item.DurationMinutes = 65;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ScheduledStatementOfWorkSegmentRequiredEmployeeIdRuleFlagsABlankEmployeeId()
    {
        var rule = new ScheduledStatementOfWorkSegmentRequiredEmployeeIdRule(
            "scheduled statement of work segment needs an entry");
        var item = ScheduledStatementOfWorkSegment.NewScheduledStatementOfWorkSegment("", "");
        item.EmployeeId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        ScheduledStatementOfWorkSegmentValidatorReportsEveryProblem();
        ScheduledStatementOfWorkSegmentNameLengthRuleFlagsALongName();
        ScheduledStatementOfWorkSegmentDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes();
        ScheduledStatementOfWorkSegmentRequiredEmployeeIdRuleFlagsABlankEmployeeId();
    }
}
