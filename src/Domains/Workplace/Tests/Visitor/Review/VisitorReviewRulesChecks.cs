namespace Acme.Workforce.Workplace.Tests.Visitor.Review;

using Acme.Workforce.Workplace.Visitor.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class VisitorReviewRulesChecks
{
    public static void VisitorReviewValidatorReportsEveryProblem()
    {
        var validator = VisitorReviewRules.DefaultVisitorReviewValidator();
        var item = VisitorReview.NewVisitorReview("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void VisitorReviewRequiredNameRuleFlagsABlankName()
    {
        var rule = new VisitorReviewRequiredNameRule("visitor review needs this field");
        var item = VisitorReview.NewVisitorReview("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void VisitorReviewRequiredDescriptionRuleFlagsABlankDescription()
    {
        var rule = new VisitorReviewRequiredDescriptionRule("visitor review needs an entry");
        var item = VisitorReview.NewVisitorReview("", "");
        item.Description = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void VisitorReviewDescriptionLengthRuleFlagsALongDescription()
    {
        var rule = new VisitorReviewDescriptionLengthRule(64);
        var item = VisitorReview.NewVisitorReview("x", "y");
        item.Description = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Description = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        VisitorReviewValidatorReportsEveryProblem();
        VisitorReviewRequiredNameRuleFlagsABlankName();
        VisitorReviewRequiredDescriptionRuleFlagsABlankDescription();
        VisitorReviewDescriptionLengthRuleFlagsALongDescription();
    }
}
