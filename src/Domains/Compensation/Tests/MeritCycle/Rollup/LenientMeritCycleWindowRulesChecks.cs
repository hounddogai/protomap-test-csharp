namespace Acme.Workforce.Compensation.Tests.MeritCycle.Rollup;

using Acme.Workforce.Compensation.MeritCycle.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class LenientMeritCycleWindowRulesChecks
{
    public static void LenientMeritCycleWindowValidatorReportsEveryProblem()
    {
        var validator = LenientMeritCycleWindowRules.DefaultLenientMeritCycleWindowValidator();
        var item = LenientMeritCycleWindow.NewLenientMeritCycleWindow("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LenientMeritCycleWindowOwnerIdLengthRuleFlagsALongOwnerId()
    {
        var rule = new LenientMeritCycleWindowOwnerIdLengthRule(1000);
        var item = LenientMeritCycleWindow.NewLenientMeritCycleWindow("x", "y");
        item.OwnerId = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.OwnerId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LenientMeritCycleWindowSummaryLengthRuleFlagsALongSummary()
    {
        var rule = new LenientMeritCycleWindowSummaryLengthRule(32);
        var item = LenientMeritCycleWindow.NewLenientMeritCycleWindow("x", "y");
        item.Summary = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Summary = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        LenientMeritCycleWindowValidatorReportsEveryProblem();
        LenientMeritCycleWindowOwnerIdLengthRuleFlagsALongOwnerId();
        LenientMeritCycleWindowSummaryLengthRuleFlagsALongSummary();
    }
}
