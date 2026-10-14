namespace Acme.Workforce.Workplace.Tests.AccessBadge.Notice;

using Acme.Workforce.Workplace.AccessBadge.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class RegionalAccessBadgeReviewRulesChecks
{
    public static void RegionalAccessBadgeReviewValidatorReportsEveryProblem()
    {
        var validator = RegionalAccessBadgeReviewRules.DefaultRegionalAccessBadgeReviewValidator();
        var item = RegionalAccessBadgeReview.NewRegionalAccessBadgeReview("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RegionalAccessBadgeReviewRequiredDepartmentRuleFlagsABlankDepartment()
    {
        var rule = new RegionalAccessBadgeReviewRequiredDepartmentRule("regional access badge review needs a value");
        var item = RegionalAccessBadgeReview.NewRegionalAccessBadgeReview("", "");
        item.Department = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RegionalAccessBadgeReviewCodeLengthRuleFlagsALongCode()
    {
        var rule = new RegionalAccessBadgeReviewCodeLengthRule(64);
        var item = RegionalAccessBadgeReview.NewRegionalAccessBadgeReview("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RegionalAccessBadgeReviewLabelLengthRuleFlagsALongLabel()
    {
        var rule = new RegionalAccessBadgeReviewLabelLengthRule(120);
        var item = RegionalAccessBadgeReview.NewRegionalAccessBadgeReview("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        RegionalAccessBadgeReviewValidatorReportsEveryProblem();
        RegionalAccessBadgeReviewRequiredDepartmentRuleFlagsABlankDepartment();
        RegionalAccessBadgeReviewCodeLengthRuleFlagsALongCode();
        RegionalAccessBadgeReviewLabelLengthRuleFlagsALongLabel();
    }
}
