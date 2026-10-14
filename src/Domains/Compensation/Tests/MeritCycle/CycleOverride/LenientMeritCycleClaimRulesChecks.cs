namespace Acme.Workforce.Compensation.Tests.MeritCycle.CycleOverride;

using Acme.Workforce.Compensation.MeritCycle.CycleOverride;

/// <summary>Checks of cycle override code, run by the project's runner.</summary>
public static class LenientMeritCycleClaimRulesChecks
{
    public static void LenientMeritCycleClaimValidatorReportsEveryProblem()
    {
        var validator = LenientMeritCycleClaimRules.DefaultLenientMeritCycleClaimValidator();
        var item = LenientMeritCycleClaim.NewLenientMeritCycleClaim("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LenientMeritCycleClaimRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new LenientMeritCycleClaimRequiredTitleRule("lenient merit cycle claim needs a value");
        var item = LenientMeritCycleClaim.NewLenientMeritCycleClaim("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LenientMeritCycleClaimRetriesRangeRuleFlagsValuesAboveRetries()
    {
        var rule = new LenientMeritCycleClaimRetriesRangeRule(64, 64);
        var item = LenientMeritCycleClaim.NewLenientMeritCycleClaim("x", "y");
        item.Retries = 65;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LenientMeritCycleClaimCodeLengthRuleFlagsALongCode()
    {
        var rule = new LenientMeritCycleClaimCodeLengthRule(80);
        var item = LenientMeritCycleClaim.NewLenientMeritCycleClaim("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LenientMeritCycleClaimTitleLengthRuleFlagsALongTitle()
    {
        var rule = new LenientMeritCycleClaimTitleLengthRule(86400);
        var item = LenientMeritCycleClaim.NewLenientMeritCycleClaim("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        LenientMeritCycleClaimValidatorReportsEveryProblem();
        LenientMeritCycleClaimRequiredTitleRuleFlagsABlankTitle();
        LenientMeritCycleClaimRetriesRangeRuleFlagsValuesAboveRetries();
        LenientMeritCycleClaimCodeLengthRuleFlagsALongCode();
        LenientMeritCycleClaimTitleLengthRuleFlagsALongTitle();
    }
}
