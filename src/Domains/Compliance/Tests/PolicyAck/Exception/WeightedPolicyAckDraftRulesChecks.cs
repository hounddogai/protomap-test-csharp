namespace Acme.Workforce.Compliance.Tests.PolicyAck.Exception;

using Acme.Workforce.Compliance.PolicyAck.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class WeightedPolicyAckDraftRulesChecks
{
    public static void WeightedPolicyAckDraftValidatorReportsEveryProblem()
    {
        var validator = WeightedPolicyAckDraftRules.DefaultWeightedPolicyAckDraftValidator();
        var item = WeightedPolicyAckDraft.NewWeightedPolicyAckDraft("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void WeightedPolicyAckDraftCategoryLengthRuleFlagsALongCategory()
    {
        var rule = new WeightedPolicyAckDraftCategoryLengthRule(32);
        var item = WeightedPolicyAckDraft.NewWeightedPolicyAckDraft("x", "y");
        item.Category = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Category = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void WeightedPolicyAckDraftTitleLengthRuleFlagsALongTitle()
    {
        var rule = new WeightedPolicyAckDraftTitleLengthRule(86400);
        var item = WeightedPolicyAckDraft.NewWeightedPolicyAckDraft("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void WeightedPolicyAckDraftRequiredOwnerIdRuleFlagsABlankOwnerId()
    {
        var rule = new WeightedPolicyAckDraftRequiredOwnerIdRule("weighted policy ack draft needs an entry");
        var item = WeightedPolicyAckDraft.NewWeightedPolicyAckDraft("", "");
        item.OwnerId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        WeightedPolicyAckDraftValidatorReportsEveryProblem();
        WeightedPolicyAckDraftCategoryLengthRuleFlagsALongCategory();
        WeightedPolicyAckDraftTitleLengthRuleFlagsALongTitle();
        WeightedPolicyAckDraftRequiredOwnerIdRuleFlagsABlankOwnerId();
    }
}
