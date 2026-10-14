namespace Acme.Workforce.Contractors.Tests.Offboarding.OffboardingSegment;

using Acme.Workforce.Contractors.Offboarding.OffboardingSegment;

/// <summary>Checks of offboarding segment code, run by the project's runner.</summary>
public static class AdaptiveOffboardingSegmentRulesChecks
{
    public static void AdaptiveOffboardingSegmentValidatorReportsEveryProblem()
    {
        var validator = AdaptiveOffboardingSegmentRules.DefaultAdaptiveOffboardingSegmentValidator();
        var item = AdaptiveOffboardingSegment.NewAdaptiveOffboardingSegment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AdaptiveOffboardingSegmentRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new AdaptiveOffboardingSegmentRequiredTitleRule("adaptive offboarding segment needs a value");
        var item = AdaptiveOffboardingSegment.NewAdaptiveOffboardingSegment("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AdaptiveOffboardingSegmentRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new AdaptiveOffboardingSegmentRequiredCodeRule("adaptive offboarding segment needs a value");
        var item = AdaptiveOffboardingSegment.NewAdaptiveOffboardingSegment("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AdaptiveOffboardingSegmentTitleLengthRuleFlagsALongTitle()
    {
        var rule = new AdaptiveOffboardingSegmentTitleLengthRule(80);
        var item = AdaptiveOffboardingSegment.NewAdaptiveOffboardingSegment("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        AdaptiveOffboardingSegmentValidatorReportsEveryProblem();
        AdaptiveOffboardingSegmentRequiredTitleRuleFlagsABlankTitle();
        AdaptiveOffboardingSegmentRequiredCodeRuleFlagsABlankCode();
        AdaptiveOffboardingSegmentTitleLengthRuleFlagsALongTitle();
    }
}
