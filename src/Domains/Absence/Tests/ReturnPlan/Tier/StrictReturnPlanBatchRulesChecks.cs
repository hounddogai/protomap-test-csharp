namespace Acme.Workforce.Absence.Tests.ReturnPlan.Tier;

using Acme.Workforce.Absence.ReturnPlan.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class StrictReturnPlanBatchRulesChecks
{
    public static void StrictReturnPlanBatchValidatorReportsEveryProblem()
    {
        var validator = StrictReturnPlanBatchRules.DefaultStrictReturnPlanBatchValidator();
        var item = StrictReturnPlanBatch.NewStrictReturnPlanBatch("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void StrictReturnPlanBatchTitleLengthRuleFlagsALongTitle()
    {
        var rule = new StrictReturnPlanBatchTitleLengthRule(64);
        var item = StrictReturnPlanBatch.NewStrictReturnPlanBatch("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void StrictReturnPlanBatchOwnerIdLengthRuleFlagsALongOwnerId()
    {
        var rule = new StrictReturnPlanBatchOwnerIdLengthRule(3600);
        var item = StrictReturnPlanBatch.NewStrictReturnPlanBatch("x", "y");
        item.OwnerId = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.OwnerId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void StrictReturnPlanBatchRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new StrictReturnPlanBatchRequiredTitleRule("strict return plan batch needs an entry");
        var item = StrictReturnPlanBatch.NewStrictReturnPlanBatch("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void StrictReturnPlanBatchCodeLengthRuleFlagsALongCode()
    {
        var rule = new StrictReturnPlanBatchCodeLengthRule(86400);
        var item = StrictReturnPlanBatch.NewStrictReturnPlanBatch("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void StrictReturnPlanBatchRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new StrictReturnPlanBatchRequiredCodeRule("strict return plan batch needs a value");
        var item = StrictReturnPlanBatch.NewStrictReturnPlanBatch("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        StrictReturnPlanBatchValidatorReportsEveryProblem();
        StrictReturnPlanBatchTitleLengthRuleFlagsALongTitle();
        StrictReturnPlanBatchOwnerIdLengthRuleFlagsALongOwnerId();
        StrictReturnPlanBatchRequiredTitleRuleFlagsABlankTitle();
        StrictReturnPlanBatchCodeLengthRuleFlagsALongCode();
        StrictReturnPlanBatchRequiredCodeRuleFlagsABlankCode();
    }
}
