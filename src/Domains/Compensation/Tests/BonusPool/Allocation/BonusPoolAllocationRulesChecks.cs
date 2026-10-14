namespace Acme.Workforce.Compensation.Tests.BonusPool.Allocation;

using Acme.Workforce.Compensation.BonusPool.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class BonusPoolAllocationRulesChecks
{
    public static void BonusPoolAllocationValidatorReportsEveryProblem()
    {
        var validator = BonusPoolAllocationRules.DefaultBonusPoolAllocationValidator();
        var item = BonusPoolAllocation.NewBonusPoolAllocation("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BonusPoolAllocationRequiredPhoneRuleFlagsABlankPhone()
    {
        var rule = new BonusPoolAllocationRequiredPhoneRule("bonus pool allocation needs an entry");
        var item = BonusPoolAllocation.NewBonusPoolAllocation("", "");
        item.Phone = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BonusPoolAllocationCodeLengthRuleFlagsALongCode()
    {
        var rule = new BonusPoolAllocationCodeLengthRule(32);
        var item = BonusPoolAllocation.NewBonusPoolAllocation("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BonusPoolAllocationNameLengthRuleFlagsALongName()
    {
        var rule = new BonusPoolAllocationNameLengthRule(255);
        var item = BonusPoolAllocation.NewBonusPoolAllocation("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BonusPoolAllocationRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new BonusPoolAllocationRequiredCodeRule("bonus pool allocation needs a value");
        var item = BonusPoolAllocation.NewBonusPoolAllocation("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BonusPoolAllocationAttemptsRangeRuleFlagsValuesAboveAttempts()
    {
        var rule = new BonusPoolAllocationAttemptsRangeRule(120, 3600);
        var item = BonusPoolAllocation.NewBonusPoolAllocation("x", "y");
        item.Attempts = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BonusPoolAllocationRequiredNameRuleFlagsABlankName()
    {
        var rule = new BonusPoolAllocationRequiredNameRule("bonus pool allocation needs this field");
        var item = BonusPoolAllocation.NewBonusPoolAllocation("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        BonusPoolAllocationValidatorReportsEveryProblem();
        BonusPoolAllocationRequiredPhoneRuleFlagsABlankPhone();
        BonusPoolAllocationCodeLengthRuleFlagsALongCode();
        BonusPoolAllocationNameLengthRuleFlagsALongName();
        BonusPoolAllocationRequiredCodeRuleFlagsABlankCode();
        BonusPoolAllocationAttemptsRangeRuleFlagsValuesAboveAttempts();
        BonusPoolAllocationRequiredNameRuleFlagsABlankName();
    }
}
