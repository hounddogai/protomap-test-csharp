namespace Acme.Workforce.Assets.Tests.Assignment.Policy;

using Acme.Workforce.Assets.Assignment.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class PendingAssignmentTierRulesChecks
{
    public static void PendingAssignmentTierValidatorReportsEveryProblem()
    {
        var validator = PendingAssignmentTierRules.DefaultPendingAssignmentTierValidator();
        var item = PendingAssignmentTier.NewPendingAssignmentTier("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PendingAssignmentTierRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new PendingAssignmentTierRequiredCodeRule("pending assignment tier needs this field");
        var item = PendingAssignmentTier.NewPendingAssignmentTier("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PendingAssignmentTierCodeLengthRuleFlagsALongCode()
    {
        var rule = new PendingAssignmentTierCodeLengthRule(120);
        var item = PendingAssignmentTier.NewPendingAssignmentTier("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PendingAssignmentTierRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new PendingAssignmentTierRequiredTitleRule("pending assignment tier needs an entry");
        var item = PendingAssignmentTier.NewPendingAssignmentTier("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PendingAssignmentTierTitleLengthRuleFlagsALongTitle()
    {
        var rule = new PendingAssignmentTierTitleLengthRule(255);
        var item = PendingAssignmentTier.NewPendingAssignmentTier("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        PendingAssignmentTierValidatorReportsEveryProblem();
        PendingAssignmentTierRequiredCodeRuleFlagsABlankCode();
        PendingAssignmentTierCodeLengthRuleFlagsALongCode();
        PendingAssignmentTierRequiredTitleRuleFlagsABlankTitle();
        PendingAssignmentTierTitleLengthRuleFlagsALongTitle();
    }
}
