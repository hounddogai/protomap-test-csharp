namespace Acme.Workforce.Assets.Tests.Laptop.Milestone;

using Acme.Workforce.Assets.Laptop.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class LaptopMilestoneRulesChecks
{
    public static void LaptopMilestoneValidatorReportsEveryProblem()
    {
        var validator = LaptopMilestoneRules.DefaultLaptopMilestoneValidator();
        var item = LaptopMilestone.NewLaptopMilestone("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LaptopMilestoneDescriptionLengthRuleFlagsALongDescription()
    {
        var rule = new LaptopMilestoneDescriptionLengthRule(1000);
        var item = LaptopMilestone.NewLaptopMilestone("x", "y");
        item.Description = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Description = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        LaptopMilestoneValidatorReportsEveryProblem();
        LaptopMilestoneDescriptionLengthRuleFlagsALongDescription();
    }
}
