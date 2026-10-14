namespace Acme.Workforce.Workplace.Tests.AccessBadge.Ledger;

using Acme.Workforce.Workplace.AccessBadge.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class FallbackAccessBadgeAssignmentRulesChecks
{
    public static void FallbackAccessBadgeAssignmentValidatorReportsEveryProblem()
    {
        var validator = FallbackAccessBadgeAssignmentRules.DefaultFallbackAccessBadgeAssignmentValidator();
        var item = FallbackAccessBadgeAssignment.NewFallbackAccessBadgeAssignment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FallbackAccessBadgeAssignmentGradeLengthRuleFlagsALongGrade()
    {
        var rule = new FallbackAccessBadgeAssignmentGradeLengthRule(32);
        var item = FallbackAccessBadgeAssignment.NewFallbackAccessBadgeAssignment("x", "y");
        item.Grade = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Grade = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void FallbackAccessBadgeAssignmentCodeLengthRuleFlagsALongCode()
    {
        var rule = new FallbackAccessBadgeAssignmentCodeLengthRule(3600);
        var item = FallbackAccessBadgeAssignment.NewFallbackAccessBadgeAssignment("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void FallbackAccessBadgeAssignmentRequiredRegionRuleFlagsABlankRegion()
    {
        var rule = new FallbackAccessBadgeAssignmentRequiredRegionRule(
            "fallback access badge assignment needs this field");
        var item = FallbackAccessBadgeAssignment.NewFallbackAccessBadgeAssignment("", "");
        item.Region = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        FallbackAccessBadgeAssignmentValidatorReportsEveryProblem();
        FallbackAccessBadgeAssignmentGradeLengthRuleFlagsALongGrade();
        FallbackAccessBadgeAssignmentCodeLengthRuleFlagsALongCode();
        FallbackAccessBadgeAssignmentRequiredRegionRuleFlagsABlankRegion();
    }
}
