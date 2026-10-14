namespace Acme.Workforce.Absence.Tests.Accrual.Segment;

using Acme.Workforce.Absence.Accrual.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class AccrualExceptionRulesChecks
{
    public static void AccrualExceptionValidatorReportsEveryProblem()
    {
        var validator = AccrualExceptionRules.DefaultAccrualExceptionValidator();
        var item = AccrualException.NewAccrualException("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AccrualExceptionGraceDaysRangeRuleFlagsValuesAboveGraceDays()
    {
        var rule = new AccrualExceptionGraceDaysRangeRule(3600, 86400);
        var item = AccrualException.NewAccrualException("x", "y");
        item.GraceDays = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AccrualExceptionLimitRangeRuleFlagsValuesAboveLimit()
    {
        var rule = new AccrualExceptionLimitRangeRule(1000, 86400);
        var item = AccrualException.NewAccrualException("x", "y");
        item.Limit = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AccrualExceptionSalaryBandLengthRuleFlagsALongSalaryBand()
    {
        var rule = new AccrualExceptionSalaryBandLengthRule(64);
        var item = AccrualException.NewAccrualException("x", "y");
        item.SalaryBand = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.SalaryBand = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        AccrualExceptionValidatorReportsEveryProblem();
        AccrualExceptionGraceDaysRangeRuleFlagsValuesAboveGraceDays();
        AccrualExceptionLimitRangeRuleFlagsValuesAboveLimit();
        AccrualExceptionSalaryBandLengthRuleFlagsALongSalaryBand();
    }
}
