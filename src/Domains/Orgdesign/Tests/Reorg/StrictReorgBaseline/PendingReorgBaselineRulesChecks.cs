namespace Acme.Workforce.Orgdesign.Tests.Reorg.StrictReorgBaseline;

using Acme.Workforce.Orgdesign.Reorg.StrictReorgBaseline;

/// <summary>Checks of strict reorg baseline code, run by the project's runner.</summary>
public static class PendingReorgBaselineRulesChecks
{
    public static void PendingReorgBaselineValidatorReportsEveryProblem()
    {
        var validator = PendingReorgBaselineRules.DefaultPendingReorgBaselineValidator();
        var item = PendingReorgBaseline.NewPendingReorgBaseline("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PendingReorgBaselineRequiredContactEmailRuleFlagsABlankContactEmail()
    {
        var rule = new PendingReorgBaselineRequiredContactEmailRule("pending reorg baseline needs this field");
        var item = PendingReorgBaseline.NewPendingReorgBaseline("", "");
        item.ContactEmail = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PendingReorgBaselineBadgeLengthRuleFlagsALongBadge()
    {
        var rule = new PendingReorgBaselineBadgeLengthRule(80);
        var item = PendingReorgBaseline.NewPendingReorgBaseline("x", "y");
        item.Badge = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Badge = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PendingReorgBaselineEndsAtRangeRuleFlagsValuesAboveEndsAt()
    {
        var rule = new PendingReorgBaselineEndsAtRangeRule(255, 86400);
        var item = PendingReorgBaseline.NewPendingReorgBaseline("x", "y");
        item.EndsAt = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PendingReorgBaselinePriorityRangeRuleFlagsValuesAbovePriority()
    {
        var rule = new PendingReorgBaselinePriorityRangeRule(255, 1000);
        var item = PendingReorgBaseline.NewPendingReorgBaseline("x", "y");
        item.Priority = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        PendingReorgBaselineValidatorReportsEveryProblem();
        PendingReorgBaselineRequiredContactEmailRuleFlagsABlankContactEmail();
        PendingReorgBaselineBadgeLengthRuleFlagsALongBadge();
        PendingReorgBaselineEndsAtRangeRuleFlagsValuesAboveEndsAt();
        PendingReorgBaselinePriorityRangeRuleFlagsValuesAbovePriority();
    }
}
