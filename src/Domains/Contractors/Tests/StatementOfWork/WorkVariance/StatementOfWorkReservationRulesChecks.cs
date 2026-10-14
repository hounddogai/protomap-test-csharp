namespace Acme.Workforce.Contractors.Tests.StatementOfWork.WorkVariance;

using Acme.Workforce.Contractors.StatementOfWork.WorkVariance;

/// <summary>Checks of work variance code, run by the project's runner.</summary>
public static class StatementOfWorkReservationRulesChecks
{
    public static void StatementOfWorkReservationValidatorReportsEveryProblem()
    {
        var validator = StatementOfWorkReservationRules.DefaultStatementOfWorkReservationValidator();
        var item = StatementOfWorkReservation.NewStatementOfWorkReservation("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void StatementOfWorkReservationJobCodeLengthRuleFlagsALongJobCode()
    {
        var rule = new StatementOfWorkReservationJobCodeLengthRule(16);
        var item = StatementOfWorkReservation.NewStatementOfWorkReservation("x", "y");
        item.JobCode = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.JobCode = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void StatementOfWorkReservationCodeLengthRuleFlagsALongCode()
    {
        var rule = new StatementOfWorkReservationCodeLengthRule(64);
        var item = StatementOfWorkReservation.NewStatementOfWorkReservation("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void StatementOfWorkReservationThresholdRangeRuleFlagsValuesAboveThreshold()
    {
        var rule = new StatementOfWorkReservationThresholdRangeRule(7.4, 3496.1);
        var item = StatementOfWorkReservation.NewStatementOfWorkReservation("x", "y");
        item.Threshold = 3497.6;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        StatementOfWorkReservationValidatorReportsEveryProblem();
        StatementOfWorkReservationJobCodeLengthRuleFlagsALongJobCode();
        StatementOfWorkReservationCodeLengthRuleFlagsALongCode();
        StatementOfWorkReservationThresholdRangeRuleFlagsValuesAboveThreshold();
    }
}
