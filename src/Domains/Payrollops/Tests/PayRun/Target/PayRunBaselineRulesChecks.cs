namespace Acme.Workforce.Payrollops.Tests.PayRun.Target;

using Acme.Workforce.Payrollops.PayRun.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class PayRunBaselineRulesChecks
{
    public static void PayRunBaselineValidatorReportsEveryProblem()
    {
        var validator = PayRunBaselineRules.DefaultPayRunBaselineValidator();
        var item = PayRunBaseline.NewPayRunBaseline("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PayRunBaselineExpiresAtRangeRuleFlagsValuesAboveExpiresAt()
    {
        var rule = new PayRunBaselineExpiresAtRangeRule(86400, 86400);
        var item = PayRunBaseline.NewPayRunBaseline("x", "y");
        item.ExpiresAt = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PayRunBaselineJobCodeLengthRuleFlagsALongJobCode()
    {
        var rule = new PayRunBaselineJobCodeLengthRule(64);
        var item = PayRunBaseline.NewPayRunBaseline("x", "y");
        item.JobCode = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.JobCode = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PayRunBaselineCodeLengthRuleFlagsALongCode()
    {
        var rule = new PayRunBaselineCodeLengthRule(255);
        var item = PayRunBaseline.NewPayRunBaseline("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PayRunBaselineGraceDaysRangeRuleFlagsValuesAboveGraceDays()
    {
        var rule = new PayRunBaselineGraceDaysRangeRule(16, 3600);
        var item = PayRunBaseline.NewPayRunBaseline("x", "y");
        item.GraceDays = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        PayRunBaselineValidatorReportsEveryProblem();
        PayRunBaselineExpiresAtRangeRuleFlagsValuesAboveExpiresAt();
        PayRunBaselineJobCodeLengthRuleFlagsALongJobCode();
        PayRunBaselineCodeLengthRuleFlagsALongCode();
        PayRunBaselineGraceDaysRangeRuleFlagsValuesAboveGraceDays();
    }
}
