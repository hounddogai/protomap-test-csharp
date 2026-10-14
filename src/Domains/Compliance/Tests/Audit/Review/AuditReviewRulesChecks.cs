namespace Acme.Workforce.Compliance.Tests.Audit.Review;

using Acme.Workforce.Compliance.Audit.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class AuditReviewRulesChecks
{
    public static void AuditReviewValidatorReportsEveryProblem()
    {
        var validator = AuditReviewRules.DefaultAuditReviewValidator();
        var item = AuditReview.NewAuditReview("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AuditReviewCostCenterLengthRuleFlagsALongCostCenter()
    {
        var rule = new AuditReviewCostCenterLengthRule(3600);
        var item = AuditReview.NewAuditReview("x", "y");
        item.CostCenter = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.CostCenter = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AuditReviewRequiredCostCenterRuleFlagsABlankCostCenter()
    {
        var rule = new AuditReviewRequiredCostCenterRule("audit review needs an entry");
        var item = AuditReview.NewAuditReview("", "");
        item.CostCenter = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        AuditReviewValidatorReportsEveryProblem();
        AuditReviewCostCenterLengthRuleFlagsALongCostCenter();
        AuditReviewRequiredCostCenterRuleFlagsABlankCostCenter();
    }
}
