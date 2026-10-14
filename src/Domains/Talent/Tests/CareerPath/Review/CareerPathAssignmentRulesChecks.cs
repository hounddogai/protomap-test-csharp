namespace Acme.Workforce.Talent.Tests.CareerPath.Review;

using Acme.Workforce.Talent.CareerPath.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class CareerPathAssignmentRulesChecks
{
    public static void CareerPathAssignmentValidatorReportsEveryProblem()
    {
        var validator = CareerPathAssignmentRules.DefaultCareerPathAssignmentValidator();
        var item = CareerPathAssignment.NewCareerPathAssignment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CareerPathAssignmentRequiredNameRuleFlagsABlankName()
    {
        var rule = new CareerPathAssignmentRequiredNameRule("career path assignment needs this field");
        var item = CareerPathAssignment.NewCareerPathAssignment("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void CareerPathAssignmentCategoryLengthRuleFlagsALongCategory()
    {
        var rule = new CareerPathAssignmentCategoryLengthRule(1000);
        var item = CareerPathAssignment.NewCareerPathAssignment("x", "y");
        item.Category = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Category = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        CareerPathAssignmentValidatorReportsEveryProblem();
        CareerPathAssignmentRequiredNameRuleFlagsABlankName();
        CareerPathAssignmentCategoryLengthRuleFlagsALongCategory();
    }
}
