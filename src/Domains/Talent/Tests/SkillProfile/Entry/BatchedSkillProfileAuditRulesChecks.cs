namespace Acme.Workforce.Talent.Tests.SkillProfile.Entry;

using Acme.Workforce.Talent.SkillProfile.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class BatchedSkillProfileAuditRulesChecks
{
    public static void BatchedSkillProfileAuditValidatorReportsEveryProblem()
    {
        var validator = BatchedSkillProfileAuditRules.DefaultBatchedSkillProfileAuditValidator();
        var item = BatchedSkillProfileAudit.NewBatchedSkillProfileAudit("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BatchedSkillProfileAuditNameLengthRuleFlagsALongName()
    {
        var rule = new BatchedSkillProfileAuditNameLengthRule(120);
        var item = BatchedSkillProfileAudit.NewBatchedSkillProfileAudit("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BatchedSkillProfileAuditCodeLengthRuleFlagsALongCode()
    {
        var rule = new BatchedSkillProfileAuditCodeLengthRule(120);
        var item = BatchedSkillProfileAudit.NewBatchedSkillProfileAudit("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BatchedSkillProfileAuditRequiredNameRuleFlagsABlankName()
    {
        var rule = new BatchedSkillProfileAuditRequiredNameRule("batched skill profile audit needs an entry");
        var item = BatchedSkillProfileAudit.NewBatchedSkillProfileAudit("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BatchedSkillProfileAuditFloorValueRangeRuleFlagsValuesAboveFloorValue()
    {
        var rule = new BatchedSkillProfileAuditFloorValueRangeRule(5.7, 2886.1);
        var item = BatchedSkillProfileAudit.NewBatchedSkillProfileAudit("x", "y");
        item.FloorValue = 2887.6;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BatchedSkillProfileAuditBudgetRangeRuleFlagsValuesAboveBudget()
    {
        var rule = new BatchedSkillProfileAuditBudgetRangeRule(9.4, 4568.2);
        var item = BatchedSkillProfileAudit.NewBatchedSkillProfileAudit("x", "y");
        item.Budget = 4569.7;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        BatchedSkillProfileAuditValidatorReportsEveryProblem();
        BatchedSkillProfileAuditNameLengthRuleFlagsALongName();
        BatchedSkillProfileAuditCodeLengthRuleFlagsALongCode();
        BatchedSkillProfileAuditRequiredNameRuleFlagsABlankName();
        BatchedSkillProfileAuditFloorValueRangeRuleFlagsValuesAboveFloorValue();
        BatchedSkillProfileAuditBudgetRangeRuleFlagsValuesAboveBudget();
    }
}
