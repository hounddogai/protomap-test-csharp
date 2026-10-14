namespace Acme.Workforce.Compensation.Tests.MeritCycle.Claim;

using Acme.Workforce.Compensation.MeritCycle.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class FallbackMeritCycleClaimRulesChecks
{
    public static void FallbackMeritCycleClaimValidatorReportsEveryProblem()
    {
        var validator = FallbackMeritCycleClaimRules.DefaultFallbackMeritCycleClaimValidator();
        var item = FallbackMeritCycleClaim.NewFallbackMeritCycleClaim("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FallbackMeritCycleClaimSourceLengthRuleFlagsALongSource()
    {
        var rule = new FallbackMeritCycleClaimSourceLengthRule(255);
        var item = FallbackMeritCycleClaim.NewFallbackMeritCycleClaim("x", "y");
        item.Source = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Source = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void FallbackMeritCycleClaimRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new FallbackMeritCycleClaimRequiredLabelRule("fallback merit cycle claim needs an entry");
        var item = FallbackMeritCycleClaim.NewFallbackMeritCycleClaim("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FallbackMeritCycleClaimPriorityRangeRuleFlagsValuesAbovePriority()
    {
        var rule = new FallbackMeritCycleClaimPriorityRangeRule(32, 255);
        var item = FallbackMeritCycleClaim.NewFallbackMeritCycleClaim("x", "y");
        item.Priority = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void FallbackMeritCycleClaimLabelLengthRuleFlagsALongLabel()
    {
        var rule = new FallbackMeritCycleClaimLabelLengthRule(80);
        var item = FallbackMeritCycleClaim.NewFallbackMeritCycleClaim("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        FallbackMeritCycleClaimValidatorReportsEveryProblem();
        FallbackMeritCycleClaimSourceLengthRuleFlagsALongSource();
        FallbackMeritCycleClaimRequiredLabelRuleFlagsABlankLabel();
        FallbackMeritCycleClaimPriorityRangeRuleFlagsValuesAbovePriority();
        FallbackMeritCycleClaimLabelLengthRuleFlagsALongLabel();
    }
}
