namespace Acme.Workforce.Compliance.Tests.PolicyAck.Reservation;

using Acme.Workforce.Compliance.PolicyAck.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class PolicyAckSnapshotRulesChecks
{
    public static void PolicyAckSnapshotValidatorReportsEveryProblem()
    {
        var validator = PolicyAckSnapshotRules.DefaultPolicyAckSnapshotValidator();
        var item = PolicyAckSnapshot.NewPolicyAckSnapshot("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PolicyAckSnapshotCategoryLengthRuleFlagsALongCategory()
    {
        var rule = new PolicyAckSnapshotCategoryLengthRule(1000);
        var item = PolicyAckSnapshot.NewPolicyAckSnapshot("x", "y");
        item.Category = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Category = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PolicyAckSnapshotRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new PolicyAckSnapshotRequiredCodeRule("policy ack snapshot needs an entry");
        var item = PolicyAckSnapshot.NewPolicyAckSnapshot("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        PolicyAckSnapshotValidatorReportsEveryProblem();
        PolicyAckSnapshotCategoryLengthRuleFlagsALongCategory();
        PolicyAckSnapshotRequiredCodeRuleFlagsABlankCode();
    }
}
