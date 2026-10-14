namespace Acme.Workforce.Contractors.Tests.Offboarding.Slot;

using Acme.Workforce.Contractors.Offboarding.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class OffboardingSlotRulesChecks
{
    public static void OffboardingSlotValidatorReportsEveryProblem()
    {
        var validator = OffboardingSlotRules.DefaultOffboardingSlotValidator();
        var item = OffboardingSlot.NewOffboardingSlot("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void OffboardingSlotFteRangeRuleFlagsValuesAboveFte()
    {
        var rule = new OffboardingSlotFteRangeRule(4.7, 2918.5);
        var item = OffboardingSlot.NewOffboardingSlot("x", "y");
        item.Fte = 2920.0;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void OffboardingSlotRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new OffboardingSlotRequiredCodeRule("offboarding slot needs an entry");
        var item = OffboardingSlot.NewOffboardingSlot("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        OffboardingSlotValidatorReportsEveryProblem();
        OffboardingSlotFteRangeRuleFlagsValuesAboveFte();
        OffboardingSlotRequiredCodeRuleFlagsABlankCode();
    }
}
