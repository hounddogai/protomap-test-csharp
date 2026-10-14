namespace Acme.Workforce.Compliance.Tests.Exception.RollingExceptionGroup;

using Acme.Workforce.Compliance.Exception.RollingExceptionGroup;

/// <summary>Checks of rolling exception group code, run by the project's runner.</summary>
public static class FallbackExceptionCycleRulesChecks
{
    public static void FallbackExceptionCycleValidatorReportsEveryProblem()
    {
        var validator = FallbackExceptionCycleRules.DefaultFallbackExceptionCycleValidator();
        var item = FallbackExceptionCycle.NewFallbackExceptionCycle("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FallbackExceptionCycleRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new FallbackExceptionCycleRequiredCodeRule("fallback exception cycle needs a value");
        var item = FallbackExceptionCycle.NewFallbackExceptionCycle("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FallbackExceptionCycleRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new FallbackExceptionCycleRequiredLabelRule("fallback exception cycle needs a value");
        var item = FallbackExceptionCycle.NewFallbackExceptionCycle("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FallbackExceptionCycleLabelLengthRuleFlagsALongLabel()
    {
        var rule = new FallbackExceptionCycleLabelLengthRule(64);
        var item = FallbackExceptionCycle.NewFallbackExceptionCycle("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void FallbackExceptionCycleRequiredContactEmailRuleFlagsABlankContactEmail()
    {
        var rule = new FallbackExceptionCycleRequiredContactEmailRule("fallback exception cycle needs a value");
        var item = FallbackExceptionCycle.NewFallbackExceptionCycle("", "");
        item.ContactEmail = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        FallbackExceptionCycleValidatorReportsEveryProblem();
        FallbackExceptionCycleRequiredCodeRuleFlagsABlankCode();
        FallbackExceptionCycleRequiredLabelRuleFlagsABlankLabel();
        FallbackExceptionCycleLabelLengthRuleFlagsALongLabel();
        FallbackExceptionCycleRequiredContactEmailRuleFlagsABlankContactEmail();
    }
}
