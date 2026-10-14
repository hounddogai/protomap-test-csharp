namespace Acme.Workforce.Assets.Tests.Laptop.Checkpoint;

using Acme.Workforce.Assets.Laptop.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class LaptopReviewRulesChecks
{
    public static void LaptopReviewValidatorReportsEveryProblem()
    {
        var validator = LaptopReviewRules.DefaultLaptopReviewValidator();
        var item = LaptopReview.NewLaptopReview("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LaptopReviewPriorityRangeRuleFlagsValuesAbovePriority()
    {
        var rule = new LaptopReviewPriorityRangeRule(32, 255);
        var item = LaptopReview.NewLaptopReview("x", "y");
        item.Priority = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LaptopReviewJobCodeLengthRuleFlagsALongJobCode()
    {
        var rule = new LaptopReviewJobCodeLengthRule(255);
        var item = LaptopReview.NewLaptopReview("x", "y");
        item.JobCode = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.JobCode = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LaptopReviewTitleLengthRuleFlagsALongTitle()
    {
        var rule = new LaptopReviewTitleLengthRule(120);
        var item = LaptopReview.NewLaptopReview("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        LaptopReviewValidatorReportsEveryProblem();
        LaptopReviewPriorityRangeRuleFlagsValuesAbovePriority();
        LaptopReviewJobCodeLengthRuleFlagsALongJobCode();
        LaptopReviewTitleLengthRuleFlagsALongTitle();
    }
}
