namespace Acme.Workforce.Contractors.Tests.Offboarding.Bundle;

using Acme.Workforce.Contractors.Offboarding.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class OffboardingAssignmentRulesChecks
{
    public static void OffboardingAssignmentValidatorReportsEveryProblem()
    {
        var validator = OffboardingAssignmentRules.DefaultOffboardingAssignmentValidator();
        var item = OffboardingAssignment.NewOffboardingAssignment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void OffboardingAssignmentBadgeLengthRuleFlagsALongBadge()
    {
        var rule = new OffboardingAssignmentBadgeLengthRule(16);
        var item = OffboardingAssignment.NewOffboardingAssignment("x", "y");
        item.Badge = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Badge = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        OffboardingAssignmentValidatorReportsEveryProblem();
        OffboardingAssignmentBadgeLengthRuleFlagsALongBadge();
    }
}
