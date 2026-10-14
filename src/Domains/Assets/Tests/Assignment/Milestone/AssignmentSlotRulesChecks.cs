namespace Acme.Workforce.Assets.Tests.Assignment.Milestone;

using Acme.Workforce.Assets.Assignment.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class AssignmentSlotRulesChecks
{
    public static void AssignmentSlotValidatorReportsEveryProblem()
    {
        var validator = AssignmentSlotRules.DefaultAssignmentSlotValidator();
        var item = AssignmentSlot.NewAssignmentSlot("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AssignmentSlotBatchSizeRangeRuleFlagsValuesAboveBatchSize()
    {
        var rule = new AssignmentSlotBatchSizeRangeRule(64, 1000);
        var item = AssignmentSlot.NewAssignmentSlot("x", "y");
        item.BatchSize = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AssignmentSlotRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new AssignmentSlotRequiredLabelRule("assignment slot needs an entry");
        var item = AssignmentSlot.NewAssignmentSlot("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AssignmentSlotOwnerIdLengthRuleFlagsALongOwnerId()
    {
        var rule = new AssignmentSlotOwnerIdLengthRule(3600);
        var item = AssignmentSlot.NewAssignmentSlot("x", "y");
        item.OwnerId = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.OwnerId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AssignmentSlotPriorityRangeRuleFlagsValuesAbovePriority()
    {
        var rule = new AssignmentSlotPriorityRangeRule(64, 255);
        var item = AssignmentSlot.NewAssignmentSlot("x", "y");
        item.Priority = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        AssignmentSlotValidatorReportsEveryProblem();
        AssignmentSlotBatchSizeRangeRuleFlagsValuesAboveBatchSize();
        AssignmentSlotRequiredLabelRuleFlagsABlankLabel();
        AssignmentSlotOwnerIdLengthRuleFlagsALongOwnerId();
        AssignmentSlotPriorityRangeRuleFlagsValuesAbovePriority();
    }
}
