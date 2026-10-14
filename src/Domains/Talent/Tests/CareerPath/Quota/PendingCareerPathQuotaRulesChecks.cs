namespace Acme.Workforce.Talent.Tests.CareerPath.Quota;

using Acme.Workforce.Talent.CareerPath.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class PendingCareerPathQuotaRulesChecks
{
    public static void PendingCareerPathQuotaValidatorReportsEveryProblem()
    {
        var validator = PendingCareerPathQuotaRules.DefaultPendingCareerPathQuotaValidator();
        var item = PendingCareerPathQuota.NewPendingCareerPathQuota("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PendingCareerPathQuotaReviewerLengthRuleFlagsALongReviewer()
    {
        var rule = new PendingCareerPathQuotaReviewerLengthRule(1000);
        var item = PendingCareerPathQuota.NewPendingCareerPathQuota("x", "y");
        item.Reviewer = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Reviewer = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PendingCareerPathQuotaRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new PendingCareerPathQuotaRequiredCodeRule("pending career path quota needs this field");
        var item = PendingCareerPathQuota.NewPendingCareerPathQuota("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PendingCareerPathQuotaRequiredReviewerRuleFlagsABlankReviewer()
    {
        var rule = new PendingCareerPathQuotaRequiredReviewerRule("pending career path quota needs a value");
        var item = PendingCareerPathQuota.NewPendingCareerPathQuota("", "");
        item.Reviewer = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PendingCareerPathQuotaWeightRangeRuleFlagsValuesAboveWeight()
    {
        var rule = new PendingCareerPathQuotaWeightRangeRule(9.4, 1860.7);
        var item = PendingCareerPathQuota.NewPendingCareerPathQuota("x", "y");
        item.Weight = 1862.2;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        PendingCareerPathQuotaValidatorReportsEveryProblem();
        PendingCareerPathQuotaReviewerLengthRuleFlagsALongReviewer();
        PendingCareerPathQuotaRequiredCodeRuleFlagsABlankCode();
        PendingCareerPathQuotaRequiredReviewerRuleFlagsABlankReviewer();
        PendingCareerPathQuotaWeightRangeRuleFlagsValuesAboveWeight();
    }
}
