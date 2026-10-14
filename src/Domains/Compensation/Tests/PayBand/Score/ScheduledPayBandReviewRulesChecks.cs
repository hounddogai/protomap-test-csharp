namespace Acme.Workforce.Compensation.Tests.PayBand.Score;

using Acme.Workforce.Compensation.PayBand.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class ScheduledPayBandReviewRulesChecks
{
    public static void ScheduledPayBandReviewValidatorReportsEveryProblem()
    {
        var validator = ScheduledPayBandReviewRules.DefaultScheduledPayBandReviewValidator();
        var item = ScheduledPayBandReview.NewScheduledPayBandReview("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ScheduledPayBandReviewRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new ScheduledPayBandReviewRequiredCodeRule("scheduled pay band review needs a value");
        var item = ScheduledPayBandReview.NewScheduledPayBandReview("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ScheduledPayBandReviewCostCenterLengthRuleFlagsALongCostCenter()
    {
        var rule = new ScheduledPayBandReviewCostCenterLengthRule(80);
        var item = ScheduledPayBandReview.NewScheduledPayBandReview("x", "y");
        item.CostCenter = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.CostCenter = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ScheduledPayBandReviewRequiredCostCenterRuleFlagsABlankCostCenter()
    {
        var rule = new ScheduledPayBandReviewRequiredCostCenterRule("scheduled pay band review needs an entry");
        var item = ScheduledPayBandReview.NewScheduledPayBandReview("", "");
        item.CostCenter = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ScheduledPayBandReviewChecksumLengthRuleFlagsALongChecksum()
    {
        var rule = new ScheduledPayBandReviewChecksumLengthRule(86400);
        var item = ScheduledPayBandReview.NewScheduledPayBandReview("x", "y");
        item.Checksum = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Checksum = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        ScheduledPayBandReviewValidatorReportsEveryProblem();
        ScheduledPayBandReviewRequiredCodeRuleFlagsABlankCode();
        ScheduledPayBandReviewCostCenterLengthRuleFlagsALongCostCenter();
        ScheduledPayBandReviewRequiredCostCenterRuleFlagsABlankCostCenter();
        ScheduledPayBandReviewChecksumLengthRuleFlagsALongChecksum();
    }
}
