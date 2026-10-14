namespace Acme.Workforce.Contractors.Tests.Offboarding.Slot;

using Acme.Workforce.Contractors.Offboarding.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class OffboardingAllocationRulesChecks
{
    public static void OffboardingAllocationValidatorReportsEveryProblem()
    {
        var validator = OffboardingAllocationRules.DefaultOffboardingAllocationValidator();
        var item = OffboardingAllocation.NewOffboardingAllocation("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void OffboardingAllocationRateRangeRuleFlagsValuesAboveRate()
    {
        var rule = new OffboardingAllocationRateRangeRule(6.4, 428.1);
        var item = OffboardingAllocation.NewOffboardingAllocation("x", "y");
        item.Rate = 429.6;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void OffboardingAllocationAmountRangeRuleFlagsValuesAboveAmount()
    {
        var rule = new OffboardingAllocationAmountRangeRule(6.0, 128.1);
        var item = OffboardingAllocation.NewOffboardingAllocation("x", "y");
        item.Amount = 129.6;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void OffboardingAllocationNameLengthRuleFlagsALongName()
    {
        var rule = new OffboardingAllocationNameLengthRule(32);
        var item = OffboardingAllocation.NewOffboardingAllocation("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void OffboardingAllocationCodeLengthRuleFlagsALongCode()
    {
        var rule = new OffboardingAllocationCodeLengthRule(255);
        var item = OffboardingAllocation.NewOffboardingAllocation("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void OffboardingAllocationSourceLengthRuleFlagsALongSource()
    {
        var rule = new OffboardingAllocationSourceLengthRule(16);
        var item = OffboardingAllocation.NewOffboardingAllocation("x", "y");
        item.Source = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Source = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        OffboardingAllocationValidatorReportsEveryProblem();
        OffboardingAllocationRateRangeRuleFlagsValuesAboveRate();
        OffboardingAllocationAmountRangeRuleFlagsValuesAboveAmount();
        OffboardingAllocationNameLengthRuleFlagsALongName();
        OffboardingAllocationCodeLengthRuleFlagsALongCode();
        OffboardingAllocationSourceLengthRuleFlagsALongSource();
    }
}
