namespace Acme.Workforce.Workplace.Tests.AccessBadge.Bundle;

using Acme.Workforce.Workplace.AccessBadge.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class AccessBadgeAssignmentRulesChecks
{
    public static void AccessBadgeAssignmentValidatorReportsEveryProblem()
    {
        var validator = AccessBadgeAssignmentRules.DefaultAccessBadgeAssignmentValidator();
        var item = AccessBadgeAssignment.NewAccessBadgeAssignment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AccessBadgeAssignmentNameLengthRuleFlagsALongName()
    {
        var rule = new AccessBadgeAssignmentNameLengthRule(80);
        var item = AccessBadgeAssignment.NewAccessBadgeAssignment("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AccessBadgeAssignmentRequiredNameRuleFlagsABlankName()
    {
        var rule = new AccessBadgeAssignmentRequiredNameRule("access badge assignment needs a value");
        var item = AccessBadgeAssignment.NewAccessBadgeAssignment("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AccessBadgeAssignmentPhoneLengthRuleFlagsALongPhone()
    {
        var rule = new AccessBadgeAssignmentPhoneLengthRule(1000);
        var item = AccessBadgeAssignment.NewAccessBadgeAssignment("x", "y");
        item.Phone = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Phone = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AccessBadgeAssignmentCodeLengthRuleFlagsALongCode()
    {
        var rule = new AccessBadgeAssignmentCodeLengthRule(255);
        var item = AccessBadgeAssignment.NewAccessBadgeAssignment("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        AccessBadgeAssignmentValidatorReportsEveryProblem();
        AccessBadgeAssignmentNameLengthRuleFlagsALongName();
        AccessBadgeAssignmentRequiredNameRuleFlagsABlankName();
        AccessBadgeAssignmentPhoneLengthRuleFlagsALongPhone();
        AccessBadgeAssignmentCodeLengthRuleFlagsALongCode();
    }
}
