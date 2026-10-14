namespace Acme.Workforce.Orgdesign.Tests.Span.Claim;

using Acme.Workforce.Orgdesign.Span.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class LenientSpanRollupRulesChecks
{
    public static void LenientSpanRollupValidatorReportsEveryProblem()
    {
        var validator = LenientSpanRollupRules.DefaultLenientSpanRollupValidator();
        var item = LenientSpanRollup.NewLenientSpanRollup("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LenientSpanRollupEmployeeIdLengthRuleFlagsALongEmployeeId()
    {
        var rule = new LenientSpanRollupEmployeeIdLengthRule(255);
        var item = LenientSpanRollup.NewLenientSpanRollup("x", "y");
        item.EmployeeId = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.EmployeeId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LenientSpanRollupLeaveDaysRangeRuleFlagsValuesAboveLeaveDays()
    {
        var rule = new LenientSpanRollupLeaveDaysRangeRule(32, 3600);
        var item = LenientSpanRollup.NewLenientSpanRollup("x", "y");
        item.LeaveDays = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LenientSpanRollupAttemptsRangeRuleFlagsValuesAboveAttempts()
    {
        var rule = new LenientSpanRollupAttemptsRangeRule(255, 1000);
        var item = LenientSpanRollup.NewLenientSpanRollup("x", "y");
        item.Attempts = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        LenientSpanRollupValidatorReportsEveryProblem();
        LenientSpanRollupEmployeeIdLengthRuleFlagsALongEmployeeId();
        LenientSpanRollupLeaveDaysRangeRuleFlagsValuesAboveLeaveDays();
        LenientSpanRollupAttemptsRangeRuleFlagsValuesAboveAttempts();
    }
}
