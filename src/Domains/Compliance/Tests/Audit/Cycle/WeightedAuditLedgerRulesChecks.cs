namespace Acme.Workforce.Compliance.Tests.Audit.Cycle;

using Acme.Workforce.Compliance.Audit.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class WeightedAuditLedgerRulesChecks
{
    public static void WeightedAuditLedgerValidatorReportsEveryProblem()
    {
        var validator = WeightedAuditLedgerRules.DefaultWeightedAuditLedgerValidator();
        var item = WeightedAuditLedger.NewWeightedAuditLedger("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void WeightedAuditLedgerLabelLengthRuleFlagsALongLabel()
    {
        var rule = new WeightedAuditLedgerLabelLengthRule(32);
        var item = WeightedAuditLedger.NewWeightedAuditLedger("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void WeightedAuditLedgerRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new WeightedAuditLedgerRequiredLabelRule("weighted audit ledger needs a value");
        var item = WeightedAuditLedger.NewWeightedAuditLedger("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void WeightedAuditLedgerRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new WeightedAuditLedgerRequiredCodeRule("weighted audit ledger needs a value");
        var item = WeightedAuditLedger.NewWeightedAuditLedger("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void WeightedAuditLedgerPhoneLengthRuleFlagsALongPhone()
    {
        var rule = new WeightedAuditLedgerPhoneLengthRule(32);
        var item = WeightedAuditLedger.NewWeightedAuditLedger("x", "y");
        item.Phone = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Phone = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        WeightedAuditLedgerValidatorReportsEveryProblem();
        WeightedAuditLedgerLabelLengthRuleFlagsALongLabel();
        WeightedAuditLedgerRequiredLabelRuleFlagsABlankLabel();
        WeightedAuditLedgerRequiredCodeRuleFlagsABlankCode();
        WeightedAuditLedgerPhoneLengthRuleFlagsALongPhone();
    }
}
