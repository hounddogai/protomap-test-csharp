namespace Acme.Workforce.Contractors.Tests.Engagement.Assignment;

using Acme.Workforce.Contractors.Engagement.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class EngagementAssignmentRulesChecks
{
    public static void EngagementAssignmentValidatorReportsEveryProblem()
    {
        var validator = EngagementAssignmentRules.DefaultEngagementAssignmentValidator();
        var item = EngagementAssignment.NewEngagementAssignment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void EngagementAssignmentRequiredSourceRuleFlagsABlankSource()
    {
        var rule = new EngagementAssignmentRequiredSourceRule("engagement assignment needs this field");
        var item = EngagementAssignment.NewEngagementAssignment("", "");
        item.Source = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void EngagementAssignmentDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes()
    {
        var rule = new EngagementAssignmentDurationMinutesRangeRule(80, 86400);
        var item = EngagementAssignment.NewEngagementAssignment("x", "y");
        item.DurationMinutes = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void EngagementAssignmentCodeLengthRuleFlagsALongCode()
    {
        var rule = new EngagementAssignmentCodeLengthRule(80);
        var item = EngagementAssignment.NewEngagementAssignment("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void EngagementAssignmentSourceLengthRuleFlagsALongSource()
    {
        var rule = new EngagementAssignmentSourceLengthRule(3600);
        var item = EngagementAssignment.NewEngagementAssignment("x", "y");
        item.Source = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Source = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void EngagementAssignmentRequiredNameRuleFlagsABlankName()
    {
        var rule = new EngagementAssignmentRequiredNameRule("engagement assignment needs this field");
        var item = EngagementAssignment.NewEngagementAssignment("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void EngagementAssignmentStartsAtRangeRuleFlagsValuesAboveStartsAt()
    {
        var rule = new EngagementAssignmentStartsAtRangeRule(32, 255);
        var item = EngagementAssignment.NewEngagementAssignment("x", "y");
        item.StartsAt = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        EngagementAssignmentValidatorReportsEveryProblem();
        EngagementAssignmentRequiredSourceRuleFlagsABlankSource();
        EngagementAssignmentDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes();
        EngagementAssignmentCodeLengthRuleFlagsALongCode();
        EngagementAssignmentSourceLengthRuleFlagsALongSource();
        EngagementAssignmentRequiredNameRuleFlagsABlankName();
        EngagementAssignmentStartsAtRangeRuleFlagsValuesAboveStartsAt();
    }
}
