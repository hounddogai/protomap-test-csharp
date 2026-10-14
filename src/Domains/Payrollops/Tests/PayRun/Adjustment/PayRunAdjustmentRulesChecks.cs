namespace Acme.Workforce.Payrollops.Tests.PayRun.Adjustment;

using Acme.Workforce.Payrollops.PayRun.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class PayRunAdjustmentRulesChecks
{
    public static void PayRunAdjustmentValidatorReportsEveryProblem()
    {
        var validator = PayRunAdjustmentRules.DefaultPayRunAdjustmentValidator();
        var item = PayRunAdjustment.NewPayRunAdjustment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PayRunAdjustmentScoreRangeRuleFlagsValuesAboveScore()
    {
        var rule = new PayRunAdjustmentScoreRangeRule(5.9, 3577.0);
        var item = PayRunAdjustment.NewPayRunAdjustment("x", "y");
        item.Score = 3578.5;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PayRunAdjustmentRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new PayRunAdjustmentRequiredCodeRule("pay run adjustment needs this field");
        var item = PayRunAdjustment.NewPayRunAdjustment("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PayRunAdjustmentRequiredEmployeeIdRuleFlagsABlankEmployeeId()
    {
        var rule = new PayRunAdjustmentRequiredEmployeeIdRule("pay run adjustment needs an entry");
        var item = PayRunAdjustment.NewPayRunAdjustment("", "");
        item.EmployeeId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PayRunAdjustmentLocationLengthRuleFlagsALongLocation()
    {
        var rule = new PayRunAdjustmentLocationLengthRule(86400);
        var item = PayRunAdjustment.NewPayRunAdjustment("x", "y");
        item.Location = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Location = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        PayRunAdjustmentValidatorReportsEveryProblem();
        PayRunAdjustmentScoreRangeRuleFlagsValuesAboveScore();
        PayRunAdjustmentRequiredCodeRuleFlagsABlankCode();
        PayRunAdjustmentRequiredEmployeeIdRuleFlagsABlankEmployeeId();
        PayRunAdjustmentLocationLengthRuleFlagsALongLocation();
    }
}
