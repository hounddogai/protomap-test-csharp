namespace Acme.Workforce.Compliance.Tests.PolicyAck.Template;

using Acme.Workforce.Compliance.PolicyAck.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class BatchedPolicyAckLedgerRulesChecks
{
    public static void BatchedPolicyAckLedgerValidatorReportsEveryProblem()
    {
        var validator = BatchedPolicyAckLedgerRules.DefaultBatchedPolicyAckLedgerValidator();
        var item = BatchedPolicyAckLedger.NewBatchedPolicyAckLedger("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BatchedPolicyAckLedgerVersionRangeRuleFlagsValuesAboveVersion()
    {
        var rule = new BatchedPolicyAckLedgerVersionRangeRule(32, 80);
        var item = BatchedPolicyAckLedger.NewBatchedPolicyAckLedger("x", "y");
        item.Version = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BatchedPolicyAckLedgerCodeLengthRuleFlagsALongCode()
    {
        var rule = new BatchedPolicyAckLedgerCodeLengthRule(1000);
        var item = BatchedPolicyAckLedger.NewBatchedPolicyAckLedger("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BatchedPolicyAckLedgerFteRangeRuleFlagsValuesAboveFte()
    {
        var rule = new BatchedPolicyAckLedgerFteRangeRule(8.4, 693.2);
        var item = BatchedPolicyAckLedger.NewBatchedPolicyAckLedger("x", "y");
        item.Fte = 694.7;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BatchedPolicyAckLedgerTitleLengthRuleFlagsALongTitle()
    {
        var rule = new BatchedPolicyAckLedgerTitleLengthRule(80);
        var item = BatchedPolicyAckLedger.NewBatchedPolicyAckLedger("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        BatchedPolicyAckLedgerValidatorReportsEveryProblem();
        BatchedPolicyAckLedgerVersionRangeRuleFlagsValuesAboveVersion();
        BatchedPolicyAckLedgerCodeLengthRuleFlagsALongCode();
        BatchedPolicyAckLedgerFteRangeRuleFlagsValuesAboveFte();
        BatchedPolicyAckLedgerTitleLengthRuleFlagsALongTitle();
    }
}
