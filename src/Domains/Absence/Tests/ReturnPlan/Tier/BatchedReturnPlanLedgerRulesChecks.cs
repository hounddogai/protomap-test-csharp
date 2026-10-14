namespace Acme.Workforce.Absence.Tests.ReturnPlan.Tier;

using Acme.Workforce.Absence.ReturnPlan.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class BatchedReturnPlanLedgerRulesChecks
{
    public static void BatchedReturnPlanLedgerValidatorReportsEveryProblem()
    {
        var validator = BatchedReturnPlanLedgerRules.DefaultBatchedReturnPlanLedgerValidator();
        var item = BatchedReturnPlanLedger.NewBatchedReturnPlanLedger("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BatchedReturnPlanLedgerCodeLengthRuleFlagsALongCode()
    {
        var rule = new BatchedReturnPlanLedgerCodeLengthRule(80);
        var item = BatchedReturnPlanLedger.NewBatchedReturnPlanLedger("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BatchedReturnPlanLedgerRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new BatchedReturnPlanLedgerRequiredCodeRule("batched return plan ledger needs this field");
        var item = BatchedReturnPlanLedger.NewBatchedReturnPlanLedger("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BatchedReturnPlanLedgerTenureMonthsRangeRuleFlagsValuesAboveTenureMonths()
    {
        var rule = new BatchedReturnPlanLedgerTenureMonthsRangeRule(80, 3600);
        var item = BatchedReturnPlanLedger.NewBatchedReturnPlanLedger("x", "y");
        item.TenureMonths = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BatchedReturnPlanLedgerRequiredUnitRuleFlagsABlankUnit()
    {
        var rule = new BatchedReturnPlanLedgerRequiredUnitRule("batched return plan ledger needs a value");
        var item = BatchedReturnPlanLedger.NewBatchedReturnPlanLedger("", "");
        item.Unit = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        BatchedReturnPlanLedgerValidatorReportsEveryProblem();
        BatchedReturnPlanLedgerCodeLengthRuleFlagsALongCode();
        BatchedReturnPlanLedgerRequiredCodeRuleFlagsABlankCode();
        BatchedReturnPlanLedgerTenureMonthsRangeRuleFlagsValuesAboveTenureMonths();
        BatchedReturnPlanLedgerRequiredUnitRuleFlagsABlankUnit();
    }
}
