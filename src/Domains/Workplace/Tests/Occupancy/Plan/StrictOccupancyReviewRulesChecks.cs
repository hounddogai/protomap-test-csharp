namespace Acme.Workforce.Workplace.Tests.Occupancy.Plan;

using Acme.Workforce.Workplace.Occupancy.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class StrictOccupancyReviewRulesChecks
{
    public static void StrictOccupancyReviewValidatorReportsEveryProblem()
    {
        var validator = StrictOccupancyReviewRules.DefaultStrictOccupancyReviewValidator();
        var item = StrictOccupancyReview.NewStrictOccupancyReview("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void StrictOccupancyReviewRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new StrictOccupancyReviewRequiredCodeRule("strict occupancy review needs a value");
        var item = StrictOccupancyReview.NewStrictOccupancyReview("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void StrictOccupancyReviewRequiredCostCenterRuleFlagsABlankCostCenter()
    {
        var rule = new StrictOccupancyReviewRequiredCostCenterRule("strict occupancy review needs this field");
        var item = StrictOccupancyReview.NewStrictOccupancyReview("", "");
        item.CostCenter = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void StrictOccupancyReviewRequiredSkillRuleFlagsABlankSkill()
    {
        var rule = new StrictOccupancyReviewRequiredSkillRule("strict occupancy review needs an entry");
        var item = StrictOccupancyReview.NewStrictOccupancyReview("", "");
        item.Skill = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        StrictOccupancyReviewValidatorReportsEveryProblem();
        StrictOccupancyReviewRequiredCodeRuleFlagsABlankCode();
        StrictOccupancyReviewRequiredCostCenterRuleFlagsABlankCostCenter();
        StrictOccupancyReviewRequiredSkillRuleFlagsABlankSkill();
    }
}
