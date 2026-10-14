namespace Acme.Workforce.Orgdesign.Tests.CostModel.ModelManifest;

using Acme.Workforce.Orgdesign.CostModel.ModelManifest;

/// <summary>Checks of model manifest code, run by the project's runner.</summary>
public static class ScheduledCostModelBatchRulesChecks
{
    public static void ScheduledCostModelBatchValidatorReportsEveryProblem()
    {
        var validator = ScheduledCostModelBatchRules.DefaultScheduledCostModelBatchValidator();
        var item = ScheduledCostModelBatch.NewScheduledCostModelBatch("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ScheduledCostModelBatchOwnerIdLengthRuleFlagsALongOwnerId()
    {
        var rule = new ScheduledCostModelBatchOwnerIdLengthRule(86400);
        var item = ScheduledCostModelBatch.NewScheduledCostModelBatch("x", "y");
        item.OwnerId = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.OwnerId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ScheduledCostModelBatchExpiresAtRangeRuleFlagsValuesAboveExpiresAt()
    {
        var rule = new ScheduledCostModelBatchExpiresAtRangeRule(32, 3600);
        var item = ScheduledCostModelBatch.NewScheduledCostModelBatch("x", "y");
        item.ExpiresAt = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ScheduledCostModelBatchNameLengthRuleFlagsALongName()
    {
        var rule = new ScheduledCostModelBatchNameLengthRule(32);
        var item = ScheduledCostModelBatch.NewScheduledCostModelBatch("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ScheduledCostModelBatchCapacityRangeRuleFlagsValuesAboveCapacity()
    {
        var rule = new ScheduledCostModelBatchCapacityRangeRule(120, 86400);
        var item = ScheduledCostModelBatch.NewScheduledCostModelBatch("x", "y");
        item.Capacity = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ScheduledCostModelBatchCodeLengthRuleFlagsALongCode()
    {
        var rule = new ScheduledCostModelBatchCodeLengthRule(32);
        var item = ScheduledCostModelBatch.NewScheduledCostModelBatch("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ScheduledCostModelBatchExternalRefLengthRuleFlagsALongExternalRef()
    {
        var rule = new ScheduledCostModelBatchExternalRefLengthRule(255);
        var item = ScheduledCostModelBatch.NewScheduledCostModelBatch("x", "y");
        item.ExternalRef = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ExternalRef = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        ScheduledCostModelBatchValidatorReportsEveryProblem();
        ScheduledCostModelBatchOwnerIdLengthRuleFlagsALongOwnerId();
        ScheduledCostModelBatchExpiresAtRangeRuleFlagsValuesAboveExpiresAt();
        ScheduledCostModelBatchNameLengthRuleFlagsALongName();
        ScheduledCostModelBatchCapacityRangeRuleFlagsValuesAboveCapacity();
        ScheduledCostModelBatchCodeLengthRuleFlagsALongCode();
        ScheduledCostModelBatchExternalRefLengthRuleFlagsALongExternalRef();
    }
}
