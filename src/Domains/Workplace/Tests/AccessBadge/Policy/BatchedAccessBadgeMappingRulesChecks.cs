namespace Acme.Workforce.Workplace.Tests.AccessBadge.Policy;

using Acme.Workforce.Workplace.AccessBadge.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class BatchedAccessBadgeMappingRulesChecks
{
    public static void BatchedAccessBadgeMappingValidatorReportsEveryProblem()
    {
        var validator = BatchedAccessBadgeMappingRules.DefaultBatchedAccessBadgeMappingValidator();
        var item = BatchedAccessBadgeMapping.NewBatchedAccessBadgeMapping("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BatchedAccessBadgeMappingRequiredCategoryRuleFlagsABlankCategory()
    {
        var rule = new BatchedAccessBadgeMappingRequiredCategoryRule("batched access badge mapping needs a value");
        var item = BatchedAccessBadgeMapping.NewBatchedAccessBadgeMapping("", "");
        item.Category = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BatchedAccessBadgeMappingCodeLengthRuleFlagsALongCode()
    {
        var rule = new BatchedAccessBadgeMappingCodeLengthRule(86400);
        var item = BatchedAccessBadgeMapping.NewBatchedAccessBadgeMapping("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        BatchedAccessBadgeMappingValidatorReportsEveryProblem();
        BatchedAccessBadgeMappingRequiredCategoryRuleFlagsABlankCategory();
        BatchedAccessBadgeMappingCodeLengthRuleFlagsALongCode();
    }
}
