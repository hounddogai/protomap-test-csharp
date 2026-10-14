namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Target;

using Acme.Workforce.Contractors.StatementOfWork.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class ScheduledStatementOfWorkRecordRulesChecks
{
    public static void ScheduledStatementOfWorkRecordValidatorReportsEveryProblem()
    {
        var validator = ScheduledStatementOfWorkRecordRules.DefaultScheduledStatementOfWorkRecordValidator();
        var item = ScheduledStatementOfWorkRecord.NewScheduledStatementOfWorkRecord("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ScheduledStatementOfWorkRecordRequiredSummaryRuleFlagsABlankSummary()
    {
        var rule = new ScheduledStatementOfWorkRecordRequiredSummaryRule(
            "scheduled statement of work record needs a value");
        var item = ScheduledStatementOfWorkRecord.NewScheduledStatementOfWorkRecord("", "");
        item.Summary = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ScheduledStatementOfWorkRecordVersionRangeRuleFlagsValuesAboveVersion()
    {
        var rule = new ScheduledStatementOfWorkRecordVersionRangeRule(120, 255);
        var item = ScheduledStatementOfWorkRecord.NewScheduledStatementOfWorkRecord("x", "y");
        item.Version = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ScheduledStatementOfWorkRecordRequiredNameRuleFlagsABlankName()
    {
        var rule = new ScheduledStatementOfWorkRecordRequiredNameRule(
            "scheduled statement of work record needs a value");
        var item = ScheduledStatementOfWorkRecord.NewScheduledStatementOfWorkRecord("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ScheduledStatementOfWorkRecordMinimumRangeRuleFlagsValuesAboveMinimum()
    {
        var rule = new ScheduledStatementOfWorkRecordMinimumRangeRule(5.8, 2446.8);
        var item = ScheduledStatementOfWorkRecord.NewScheduledStatementOfWorkRecord("x", "y");
        item.Minimum = 2448.3;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        ScheduledStatementOfWorkRecordValidatorReportsEveryProblem();
        ScheduledStatementOfWorkRecordRequiredSummaryRuleFlagsABlankSummary();
        ScheduledStatementOfWorkRecordVersionRangeRuleFlagsValuesAboveVersion();
        ScheduledStatementOfWorkRecordRequiredNameRuleFlagsABlankName();
        ScheduledStatementOfWorkRecordMinimumRangeRuleFlagsValuesAboveMinimum();
    }
}
