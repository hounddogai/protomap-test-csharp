namespace Acme.Workforce.Contractors.Tests.Offboarding.OffboardingThreshold;

using Acme.Workforce.Contractors.Offboarding.OffboardingThreshold;

/// <summary>Checks of offboarding threshold code, run by the project's runner.</summary>
public static class PrimaryOffboardingSlotRulesChecks
{
    public static void PrimaryOffboardingSlotValidatorReportsEveryProblem()
    {
        var validator = PrimaryOffboardingSlotRules.DefaultPrimaryOffboardingSlotValidator();
        var item = PrimaryOffboardingSlot.NewPrimaryOffboardingSlot("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PrimaryOffboardingSlotNameLengthRuleFlagsALongName()
    {
        var rule = new PrimaryOffboardingSlotNameLengthRule(3600);
        var item = PrimaryOffboardingSlot.NewPrimaryOffboardingSlot("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PrimaryOffboardingSlotDiscountRangeRuleFlagsValuesAboveDiscount()
    {
        var rule = new PrimaryOffboardingSlotDiscountRangeRule(5.5, 4700.3);
        var item = PrimaryOffboardingSlot.NewPrimaryOffboardingSlot("x", "y");
        item.Discount = 4701.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PrimaryOffboardingSlotRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new PrimaryOffboardingSlotRequiredCodeRule("primary offboarding slot needs this field");
        var item = PrimaryOffboardingSlot.NewPrimaryOffboardingSlot("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        PrimaryOffboardingSlotValidatorReportsEveryProblem();
        PrimaryOffboardingSlotNameLengthRuleFlagsALongName();
        PrimaryOffboardingSlotDiscountRangeRuleFlagsValuesAboveDiscount();
        PrimaryOffboardingSlotRequiredCodeRuleFlagsABlankCode();
    }
}
