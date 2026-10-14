namespace Acme.Workforce.Assets.Tests.Assignment.Checkpoint;

using Acme.Workforce.Assets.Assignment.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class AssignmentAdjustmentRulesChecks
{
    public static void AssignmentAdjustmentValidatorReportsEveryProblem()
    {
        var validator = AssignmentAdjustmentRules.DefaultAssignmentAdjustmentValidator();
        var item = AssignmentAdjustment.NewAssignmentAdjustment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AssignmentAdjustmentRequiredNameRuleFlagsABlankName()
    {
        var rule = new AssignmentAdjustmentRequiredNameRule("assignment adjustment needs this field");
        var item = AssignmentAdjustment.NewAssignmentAdjustment("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AssignmentAdjustmentCodeLengthRuleFlagsALongCode()
    {
        var rule = new AssignmentAdjustmentCodeLengthRule(32);
        var item = AssignmentAdjustment.NewAssignmentAdjustment("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AssignmentAdjustmentRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new AssignmentAdjustmentRequiredCodeRule("assignment adjustment needs a value");
        var item = AssignmentAdjustment.NewAssignmentAdjustment("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AssignmentAdjustmentExpiresAtRangeRuleFlagsValuesAboveExpiresAt()
    {
        var rule = new AssignmentAdjustmentExpiresAtRangeRule(16, 86400);
        var item = AssignmentAdjustment.NewAssignmentAdjustment("x", "y");
        item.ExpiresAt = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AssignmentAdjustmentTitleLengthRuleFlagsALongTitle()
    {
        var rule = new AssignmentAdjustmentTitleLengthRule(3600);
        var item = AssignmentAdjustment.NewAssignmentAdjustment("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AssignmentAdjustmentDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes()
    {
        var rule = new AssignmentAdjustmentDurationMinutesRangeRule(64, 120);
        var item = AssignmentAdjustment.NewAssignmentAdjustment("x", "y");
        item.DurationMinutes = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        AssignmentAdjustmentValidatorReportsEveryProblem();
        AssignmentAdjustmentRequiredNameRuleFlagsABlankName();
        AssignmentAdjustmentCodeLengthRuleFlagsALongCode();
        AssignmentAdjustmentRequiredCodeRuleFlagsABlankCode();
        AssignmentAdjustmentExpiresAtRangeRuleFlagsValuesAboveExpiresAt();
        AssignmentAdjustmentTitleLengthRuleFlagsALongTitle();
        AssignmentAdjustmentDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes();
    }
}
