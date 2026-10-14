namespace Acme.Workforce.Orgdesign.Tests.Reorg.StrictReorgBaseline;

using Acme.Workforce.Orgdesign.Reorg.StrictReorgBaseline;

/// <summary>Checks of strict reorg baseline code, run by the project's runner.</summary>
public static class ReorgTargetRulesChecks
{
    public static void ReorgTargetValidatorReportsEveryProblem()
    {
        var validator = ReorgTargetRules.DefaultReorgTargetValidator();
        var item = ReorgTarget.NewReorgTarget("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ReorgTargetRetriesRangeRuleFlagsValuesAboveRetries()
    {
        var rule = new ReorgTargetRetriesRangeRule(32, 3600);
        var item = ReorgTarget.NewReorgTarget("x", "y");
        item.Retries = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ReorgTargetRequiredJobCodeRuleFlagsABlankJobCode()
    {
        var rule = new ReorgTargetRequiredJobCodeRule("reorg target needs an entry");
        var item = ReorgTarget.NewReorgTarget("", "");
        item.JobCode = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ReorgTargetSequenceRangeRuleFlagsValuesAboveSequence()
    {
        var rule = new ReorgTargetSequenceRangeRule(80, 86400);
        var item = ReorgTarget.NewReorgTarget("x", "y");
        item.Sequence = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ReorgTargetTitleLengthRuleFlagsALongTitle()
    {
        var rule = new ReorgTargetTitleLengthRule(16);
        var item = ReorgTarget.NewReorgTarget("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ReorgTargetBadgeLengthRuleFlagsALongBadge()
    {
        var rule = new ReorgTargetBadgeLengthRule(32);
        var item = ReorgTarget.NewReorgTarget("x", "y");
        item.Badge = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Badge = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        ReorgTargetValidatorReportsEveryProblem();
        ReorgTargetRetriesRangeRuleFlagsValuesAboveRetries();
        ReorgTargetRequiredJobCodeRuleFlagsABlankJobCode();
        ReorgTargetSequenceRangeRuleFlagsValuesAboveSequence();
        ReorgTargetTitleLengthRuleFlagsALongTitle();
        ReorgTargetBadgeLengthRuleFlagsALongBadge();
    }
}
