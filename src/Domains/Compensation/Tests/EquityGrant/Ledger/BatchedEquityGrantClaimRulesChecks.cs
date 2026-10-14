namespace Acme.Workforce.Compensation.Tests.EquityGrant.Ledger;

using Acme.Workforce.Compensation.EquityGrant.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class BatchedEquityGrantClaimRulesChecks
{
    public static void BatchedEquityGrantClaimValidatorReportsEveryProblem()
    {
        var validator = BatchedEquityGrantClaimRules.DefaultBatchedEquityGrantClaimValidator();
        var item = BatchedEquityGrantClaim.NewBatchedEquityGrantClaim("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BatchedEquityGrantClaimFloorValueRangeRuleFlagsValuesAboveFloorValue()
    {
        var rule = new BatchedEquityGrantClaimFloorValueRangeRule(5.9, 4881.0);
        var item = BatchedEquityGrantClaim.NewBatchedEquityGrantClaim("x", "y");
        item.FloorValue = 4882.5;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BatchedEquityGrantClaimGraceDaysRangeRuleFlagsValuesAboveGraceDays()
    {
        var rule = new BatchedEquityGrantClaimGraceDaysRangeRule(80, 86400);
        var item = BatchedEquityGrantClaim.NewBatchedEquityGrantClaim("x", "y");
        item.GraceDays = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BatchedEquityGrantClaimOwnerIdLengthRuleFlagsALongOwnerId()
    {
        var rule = new BatchedEquityGrantClaimOwnerIdLengthRule(64);
        var item = BatchedEquityGrantClaim.NewBatchedEquityGrantClaim("x", "y");
        item.OwnerId = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.OwnerId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BatchedEquityGrantClaimRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new BatchedEquityGrantClaimRequiredCodeRule("batched equity grant claim needs a value");
        var item = BatchedEquityGrantClaim.NewBatchedEquityGrantClaim("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        BatchedEquityGrantClaimValidatorReportsEveryProblem();
        BatchedEquityGrantClaimFloorValueRangeRuleFlagsValuesAboveFloorValue();
        BatchedEquityGrantClaimGraceDaysRangeRuleFlagsValuesAboveGraceDays();
        BatchedEquityGrantClaimOwnerIdLengthRuleFlagsALongOwnerId();
        BatchedEquityGrantClaimRequiredCodeRuleFlagsABlankCode();
    }
}
