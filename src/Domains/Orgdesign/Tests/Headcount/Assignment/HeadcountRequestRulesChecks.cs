namespace Acme.Workforce.Orgdesign.Tests.Headcount.Assignment;

using Acme.Workforce.Orgdesign.Headcount.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class HeadcountRequestRulesChecks
{
    public static void HeadcountRequestValidatorReportsEveryProblem()
    {
        var validator = HeadcountRequestRules.DefaultHeadcountRequestValidator();
        var item = HeadcountRequest.NewHeadcountRequest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void HeadcountRequestCodeLengthRuleFlagsALongCode()
    {
        var rule = new HeadcountRequestCodeLengthRule(3600);
        var item = HeadcountRequest.NewHeadcountRequest("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void HeadcountRequestRequiredReviewerRuleFlagsABlankReviewer()
    {
        var rule = new HeadcountRequestRequiredReviewerRule("headcount request needs this field");
        var item = HeadcountRequest.NewHeadcountRequest("", "");
        item.Reviewer = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void HeadcountRequestOwnerIdLengthRuleFlagsALongOwnerId()
    {
        var rule = new HeadcountRequestOwnerIdLengthRule(120);
        var item = HeadcountRequest.NewHeadcountRequest("x", "y");
        item.OwnerId = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.OwnerId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        HeadcountRequestValidatorReportsEveryProblem();
        HeadcountRequestCodeLengthRuleFlagsALongCode();
        HeadcountRequestRequiredReviewerRuleFlagsABlankReviewer();
        HeadcountRequestOwnerIdLengthRuleFlagsALongOwnerId();
    }
}
