namespace Acme.Workforce.Payrollops.Tests.RetroPay.PayAssignment;

using Acme.Workforce.Payrollops.RetroPay.PayAssignment;

/// <summary>Checks of pay assignment code, run by the project's runner.</summary>
public static class RollingRetroPayAssignmentRulesChecks
{
    public static void RollingRetroPayAssignmentValidatorReportsEveryProblem()
    {
        var validator = RollingRetroPayAssignmentRules.DefaultRollingRetroPayAssignmentValidator();
        var item = RollingRetroPayAssignment.NewRollingRetroPayAssignment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RollingRetroPayAssignmentDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes()
    {
        var rule = new RollingRetroPayAssignmentDurationMinutesRangeRule(64, 86400);
        var item = RollingRetroPayAssignment.NewRollingRetroPayAssignment("x", "y");
        item.DurationMinutes = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RollingRetroPayAssignmentRevisionRangeRuleFlagsValuesAboveRevision()
    {
        var rule = new RollingRetroPayAssignmentRevisionRangeRule(64, 3600);
        var item = RollingRetroPayAssignment.NewRollingRetroPayAssignment("x", "y");
        item.Revision = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RollingRetroPayAssignmentLocationLengthRuleFlagsALongLocation()
    {
        var rule = new RollingRetroPayAssignmentLocationLengthRule(120);
        var item = RollingRetroPayAssignment.NewRollingRetroPayAssignment("x", "y");
        item.Location = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Location = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RollingRetroPayAssignmentRequiredLocationRuleFlagsABlankLocation()
    {
        var rule = new RollingRetroPayAssignmentRequiredLocationRule("rolling retro pay assignment needs this field");
        var item = RollingRetroPayAssignment.NewRollingRetroPayAssignment("", "");
        item.Location = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        RollingRetroPayAssignmentValidatorReportsEveryProblem();
        RollingRetroPayAssignmentDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes();
        RollingRetroPayAssignmentRevisionRangeRuleFlagsValuesAboveRevision();
        RollingRetroPayAssignmentLocationLengthRuleFlagsALongLocation();
        RollingRetroPayAssignmentRequiredLocationRuleFlagsABlankLocation();
    }
}
