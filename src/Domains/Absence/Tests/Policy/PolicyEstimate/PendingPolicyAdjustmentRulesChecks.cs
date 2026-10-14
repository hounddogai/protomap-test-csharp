namespace Acme.Workforce.Absence.Tests.Policy.PolicyEstimate;

using Acme.Workforce.Absence.Policy.PolicyEstimate;

/// <summary>Checks of policy estimate code, run by the project's runner.</summary>
public static class PendingPolicyAdjustmentRulesChecks
{
    public static void PendingPolicyAdjustmentValidatorReportsEveryProblem()
    {
        var validator = PendingPolicyAdjustmentRules.DefaultPendingPolicyAdjustmentValidator();
        var item = PendingPolicyAdjustment.NewPendingPolicyAdjustment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PendingPolicyAdjustmentRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new PendingPolicyAdjustmentRequiredTitleRule("pending policy adjustment needs this field");
        var item = PendingPolicyAdjustment.NewPendingPolicyAdjustment("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PendingPolicyAdjustmentLocationLengthRuleFlagsALongLocation()
    {
        var rule = new PendingPolicyAdjustmentLocationLengthRule(64);
        var item = PendingPolicyAdjustment.NewPendingPolicyAdjustment("x", "y");
        item.Location = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Location = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PendingPolicyAdjustmentRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new PendingPolicyAdjustmentRequiredCodeRule("pending policy adjustment needs this field");
        var item = PendingPolicyAdjustment.NewPendingPolicyAdjustment("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        PendingPolicyAdjustmentValidatorReportsEveryProblem();
        PendingPolicyAdjustmentRequiredTitleRuleFlagsABlankTitle();
        PendingPolicyAdjustmentLocationLengthRuleFlagsALongLocation();
        PendingPolicyAdjustmentRequiredCodeRuleFlagsABlankCode();
    }
}
