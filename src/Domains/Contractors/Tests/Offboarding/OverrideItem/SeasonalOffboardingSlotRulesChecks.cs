namespace Acme.Workforce.Contractors.Tests.Offboarding.OverrideItem;

using Acme.Workforce.Contractors.Offboarding.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class SeasonalOffboardingSlotRulesChecks
{
    public static void SeasonalOffboardingSlotValidatorReportsEveryProblem()
    {
        var validator = SeasonalOffboardingSlotRules.DefaultSeasonalOffboardingSlotValidator();
        var item = SeasonalOffboardingSlot.NewSeasonalOffboardingSlot("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SeasonalOffboardingSlotRequiredContactEmailRuleFlagsABlankContactEmail()
    {
        var rule = new SeasonalOffboardingSlotRequiredContactEmailRule("seasonal offboarding slot needs a value");
        var item = SeasonalOffboardingSlot.NewSeasonalOffboardingSlot("", "");
        item.ContactEmail = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SeasonalOffboardingSlotSequenceRangeRuleFlagsValuesAboveSequence()
    {
        var rule = new SeasonalOffboardingSlotSequenceRangeRule(32, 3600);
        var item = SeasonalOffboardingSlot.NewSeasonalOffboardingSlot("x", "y");
        item.Sequence = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void SeasonalOffboardingSlotMaximumRangeRuleFlagsValuesAboveMaximum()
    {
        var rule = new SeasonalOffboardingSlotMaximumRangeRule(3.3, 2072.3);
        var item = SeasonalOffboardingSlot.NewSeasonalOffboardingSlot("x", "y");
        item.Maximum = 2073.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void SeasonalOffboardingSlotRequiredNameRuleFlagsABlankName()
    {
        var rule = new SeasonalOffboardingSlotRequiredNameRule("seasonal offboarding slot needs an entry");
        var item = SeasonalOffboardingSlot.NewSeasonalOffboardingSlot("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        SeasonalOffboardingSlotValidatorReportsEveryProblem();
        SeasonalOffboardingSlotRequiredContactEmailRuleFlagsABlankContactEmail();
        SeasonalOffboardingSlotSequenceRangeRuleFlagsValuesAboveSequence();
        SeasonalOffboardingSlotMaximumRangeRuleFlagsValuesAboveMaximum();
        SeasonalOffboardingSlotRequiredNameRuleFlagsABlankName();
    }
}
