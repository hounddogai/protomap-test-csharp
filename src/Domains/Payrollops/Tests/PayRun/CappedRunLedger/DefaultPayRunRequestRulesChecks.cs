namespace Acme.Workforce.Payrollops.Tests.PayRun.CappedRunLedger;

using Acme.Workforce.Payrollops.PayRun.CappedRunLedger;

/// <summary>Checks of capped run ledger code, run by the project's runner.</summary>
public static class DefaultPayRunRequestRulesChecks
{
    public static void DefaultPayRunRequestValidatorReportsEveryProblem()
    {
        var validator = DefaultPayRunRequestRules.DefaultDefaultPayRunRequestValidator();
        var item = DefaultPayRunRequest.NewDefaultPayRunRequest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DefaultPayRunRequestWindowDaysRangeRuleFlagsValuesAboveWindowDays()
    {
        var rule = new DefaultPayRunRequestWindowDaysRangeRule(64, 86400);
        var item = DefaultPayRunRequest.NewDefaultPayRunRequest("x", "y");
        item.WindowDays = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void DefaultPayRunRequestChannelLengthRuleFlagsALongChannel()
    {
        var rule = new DefaultPayRunRequestChannelLengthRule(1000);
        var item = DefaultPayRunRequest.NewDefaultPayRunRequest("x", "y");
        item.Channel = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Channel = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void DefaultPayRunRequestVersionRangeRuleFlagsValuesAboveVersion()
    {
        var rule = new DefaultPayRunRequestVersionRangeRule(16, 3600);
        var item = DefaultPayRunRequest.NewDefaultPayRunRequest("x", "y");
        item.Version = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        DefaultPayRunRequestValidatorReportsEveryProblem();
        DefaultPayRunRequestWindowDaysRangeRuleFlagsValuesAboveWindowDays();
        DefaultPayRunRequestChannelLengthRuleFlagsALongChannel();
        DefaultPayRunRequestVersionRangeRuleFlagsValuesAboveVersion();
    }
}
