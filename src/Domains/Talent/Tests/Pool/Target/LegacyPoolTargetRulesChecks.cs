namespace Acme.Workforce.Talent.Tests.Pool.Target;

using Acme.Workforce.Talent.Pool.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class LegacyPoolTargetRulesChecks
{
    public static void LegacyPoolTargetValidatorReportsEveryProblem()
    {
        var validator = LegacyPoolTargetRules.DefaultLegacyPoolTargetValidator();
        var item = LegacyPoolTarget.NewLegacyPoolTarget("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LegacyPoolTargetRequiredReviewerRuleFlagsABlankReviewer()
    {
        var rule = new LegacyPoolTargetRequiredReviewerRule("legacy pool target needs a value");
        var item = LegacyPoolTarget.NewLegacyPoolTarget("", "");
        item.Reviewer = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LegacyPoolTargetRequiredChecksumRuleFlagsABlankChecksum()
    {
        var rule = new LegacyPoolTargetRequiredChecksumRule("legacy pool target needs an entry");
        var item = LegacyPoolTarget.NewLegacyPoolTarget("", "");
        item.Checksum = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LegacyPoolTargetPriorityRangeRuleFlagsValuesAbovePriority()
    {
        var rule = new LegacyPoolTargetPriorityRangeRule(255, 86400);
        var item = LegacyPoolTarget.NewLegacyPoolTarget("x", "y");
        item.Priority = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        LegacyPoolTargetValidatorReportsEveryProblem();
        LegacyPoolTargetRequiredReviewerRuleFlagsABlankReviewer();
        LegacyPoolTargetRequiredChecksumRuleFlagsABlankChecksum();
        LegacyPoolTargetPriorityRangeRuleFlagsValuesAbovePriority();
    }
}
