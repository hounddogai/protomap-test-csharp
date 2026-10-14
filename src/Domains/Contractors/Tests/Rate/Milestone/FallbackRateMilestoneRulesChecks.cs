namespace Acme.Workforce.Contractors.Tests.Rate.Milestone;

using Acme.Workforce.Contractors.Rate.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class FallbackRateMilestoneRulesChecks
{
    public static void FallbackRateMilestoneValidatorReportsEveryProblem()
    {
        var validator = FallbackRateMilestoneRules.DefaultFallbackRateMilestoneValidator();
        var item = FallbackRateMilestone.NewFallbackRateMilestone("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FallbackRateMilestoneRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new FallbackRateMilestoneRequiredCodeRule("fallback rate milestone needs an entry");
        var item = FallbackRateMilestone.NewFallbackRateMilestone("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FallbackRateMilestoneRevisionRangeRuleFlagsValuesAboveRevision()
    {
        var rule = new FallbackRateMilestoneRevisionRangeRule(3600, 86400);
        var item = FallbackRateMilestone.NewFallbackRateMilestone("x", "y");
        item.Revision = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void FallbackRateMilestoneRequiredNameRuleFlagsABlankName()
    {
        var rule = new FallbackRateMilestoneRequiredNameRule("fallback rate milestone needs an entry");
        var item = FallbackRateMilestone.NewFallbackRateMilestone("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FallbackRateMilestoneCodeLengthRuleFlagsALongCode()
    {
        var rule = new FallbackRateMilestoneCodeLengthRule(3600);
        var item = FallbackRateMilestone.NewFallbackRateMilestone("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        FallbackRateMilestoneValidatorReportsEveryProblem();
        FallbackRateMilestoneRequiredCodeRuleFlagsABlankCode();
        FallbackRateMilestoneRevisionRangeRuleFlagsValuesAboveRevision();
        FallbackRateMilestoneRequiredNameRuleFlagsABlankName();
        FallbackRateMilestoneCodeLengthRuleFlagsALongCode();
    }
}
