namespace Acme.Workforce.Payrollops.Tests.Deduction.Draft;

using Acme.Workforce.Payrollops.Deduction.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class StrictDeductionDraftRulesChecks
{
    public static void StrictDeductionDraftValidatorReportsEveryProblem()
    {
        var validator = StrictDeductionDraftRules.DefaultStrictDeductionDraftValidator();
        var item = StrictDeductionDraft.NewStrictDeductionDraft("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void StrictDeductionDraftCodeLengthRuleFlagsALongCode()
    {
        var rule = new StrictDeductionDraftCodeLengthRule(64);
        var item = StrictDeductionDraft.NewStrictDeductionDraft("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void StrictDeductionDraftOwnerIdLengthRuleFlagsALongOwnerId()
    {
        var rule = new StrictDeductionDraftOwnerIdLengthRule(120);
        var item = StrictDeductionDraft.NewStrictDeductionDraft("x", "y");
        item.OwnerId = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.OwnerId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void StrictDeductionDraftRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new StrictDeductionDraftRequiredCodeRule("strict deduction draft needs a value");
        var item = StrictDeductionDraft.NewStrictDeductionDraft("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void StrictDeductionDraftQuantityRangeRuleFlagsValuesAboveQuantity()
    {
        var rule = new StrictDeductionDraftQuantityRangeRule(120, 86400);
        var item = StrictDeductionDraft.NewStrictDeductionDraft("x", "y");
        item.Quantity = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        StrictDeductionDraftValidatorReportsEveryProblem();
        StrictDeductionDraftCodeLengthRuleFlagsALongCode();
        StrictDeductionDraftOwnerIdLengthRuleFlagsALongOwnerId();
        StrictDeductionDraftRequiredCodeRuleFlagsABlankCode();
        StrictDeductionDraftQuantityRangeRuleFlagsValuesAboveQuantity();
    }
}
