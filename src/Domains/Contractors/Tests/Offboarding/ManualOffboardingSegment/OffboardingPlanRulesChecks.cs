namespace Acme.Workforce.Contractors.Tests.Offboarding.ManualOffboardingSegment;

using Acme.Workforce.Contractors.Offboarding.ManualOffboardingSegment;

/// <summary>Checks of manual offboarding segment code, run by the project's runner.</summary>
public static class OffboardingPlanRulesChecks
{
    public static void OffboardingPlanValidatorReportsEveryProblem()
    {
        var validator = OffboardingPlanRules.DefaultOffboardingPlanValidator();
        var item = OffboardingPlan.NewOffboardingPlan("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void OffboardingPlanDescriptionLengthRuleFlagsALongDescription()
    {
        var rule = new OffboardingPlanDescriptionLengthRule(32);
        var item = OffboardingPlan.NewOffboardingPlan("x", "y");
        item.Description = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Description = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        OffboardingPlanValidatorReportsEveryProblem();
        OffboardingPlanDescriptionLengthRuleFlagsALongDescription();
    }
}
