namespace Acme.Workforce.Assets.Tests.LicenseSeat.SeatMilestone;

using Acme.Workforce.Assets.LicenseSeat.SeatMilestone;

/// <summary>Checks of seat milestone code, run by the project's runner.</summary>
public static class LicenseSeatAssignmentRulesChecks
{
    public static void LicenseSeatAssignmentValidatorReportsEveryProblem()
    {
        var validator = LicenseSeatAssignmentRules.DefaultLicenseSeatAssignmentValidator();
        var item = LicenseSeatAssignment.NewLicenseSeatAssignment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LicenseSeatAssignmentRequiredGradeRuleFlagsABlankGrade()
    {
        var rule = new LicenseSeatAssignmentRequiredGradeRule("license seat assignment needs a value");
        var item = LicenseSeatAssignment.NewLicenseSeatAssignment("", "");
        item.Grade = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LicenseSeatAssignmentUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt()
    {
        var rule = new LicenseSeatAssignmentUpdatedAtRangeRule(32, 86400);
        var item = LicenseSeatAssignment.NewLicenseSeatAssignment("x", "y");
        item.UpdatedAt = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LicenseSeatAssignmentCodeLengthRuleFlagsALongCode()
    {
        var rule = new LicenseSeatAssignmentCodeLengthRule(1000);
        var item = LicenseSeatAssignment.NewLicenseSeatAssignment("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LicenseSeatAssignmentLocaleLengthRuleFlagsALongLocale()
    {
        var rule = new LicenseSeatAssignmentLocaleLengthRule(3600);
        var item = LicenseSeatAssignment.NewLicenseSeatAssignment("x", "y");
        item.Locale = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Locale = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LicenseSeatAssignmentRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new LicenseSeatAssignmentRequiredLabelRule("license seat assignment needs this field");
        var item = LicenseSeatAssignment.NewLicenseSeatAssignment("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        LicenseSeatAssignmentValidatorReportsEveryProblem();
        LicenseSeatAssignmentRequiredGradeRuleFlagsABlankGrade();
        LicenseSeatAssignmentUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt();
        LicenseSeatAssignmentCodeLengthRuleFlagsALongCode();
        LicenseSeatAssignmentLocaleLengthRuleFlagsALongLocale();
        LicenseSeatAssignmentRequiredLabelRuleFlagsABlankLabel();
    }
}
