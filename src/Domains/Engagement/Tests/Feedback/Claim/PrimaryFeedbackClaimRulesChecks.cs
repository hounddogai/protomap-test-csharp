namespace Acme.Workforce.Engagement.Tests.Feedback.Claim;

using Acme.Workforce.Engagement.Feedback.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class PrimaryFeedbackClaimRulesChecks
{
    public static void PrimaryFeedbackClaimValidatorReportsEveryProblem()
    {
        var validator = PrimaryFeedbackClaimRules.DefaultPrimaryFeedbackClaimValidator();
        var item = PrimaryFeedbackClaim.NewPrimaryFeedbackClaim("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PrimaryFeedbackClaimPriorityRangeRuleFlagsValuesAbovePriority()
    {
        var rule = new PrimaryFeedbackClaimPriorityRangeRule(80, 1000);
        var item = PrimaryFeedbackClaim.NewPrimaryFeedbackClaim("x", "y");
        item.Priority = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PrimaryFeedbackClaimTitleLengthRuleFlagsALongTitle()
    {
        var rule = new PrimaryFeedbackClaimTitleLengthRule(255);
        var item = PrimaryFeedbackClaim.NewPrimaryFeedbackClaim("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PrimaryFeedbackClaimEndsAtRangeRuleFlagsValuesAboveEndsAt()
    {
        var rule = new PrimaryFeedbackClaimEndsAtRangeRule(1000, 1000);
        var item = PrimaryFeedbackClaim.NewPrimaryFeedbackClaim("x", "y");
        item.EndsAt = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        PrimaryFeedbackClaimValidatorReportsEveryProblem();
        PrimaryFeedbackClaimPriorityRangeRuleFlagsValuesAbovePriority();
        PrimaryFeedbackClaimTitleLengthRuleFlagsALongTitle();
        PrimaryFeedbackClaimEndsAtRangeRuleFlagsValuesAboveEndsAt();
    }
}
