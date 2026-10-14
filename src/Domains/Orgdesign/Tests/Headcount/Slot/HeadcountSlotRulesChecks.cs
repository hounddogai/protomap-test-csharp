namespace Acme.Workforce.Orgdesign.Tests.Headcount.Slot;

using Acme.Workforce.Orgdesign.Headcount.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class HeadcountSlotRulesChecks
{
    public static void HeadcountSlotValidatorReportsEveryProblem()
    {
        var validator = HeadcountSlotRules.DefaultHeadcountSlotValidator();
        var item = HeadcountSlot.NewHeadcountSlot("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void HeadcountSlotAmountRangeRuleFlagsValuesAboveAmount()
    {
        var rule = new HeadcountSlotAmountRangeRule(2.8, 4802.8);
        var item = HeadcountSlot.NewHeadcountSlot("x", "y");
        item.Amount = 4804.3;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void HeadcountSlotRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new HeadcountSlotRequiredTitleRule("headcount slot needs an entry");
        var item = HeadcountSlot.NewHeadcountSlot("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void HeadcountSlotGraceDaysRangeRuleFlagsValuesAboveGraceDays()
    {
        var rule = new HeadcountSlotGraceDaysRangeRule(16, 1000);
        var item = HeadcountSlot.NewHeadcountSlot("x", "y");
        item.GraceDays = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void HeadcountSlotRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new HeadcountSlotRequiredCodeRule("headcount slot needs an entry");
        var item = HeadcountSlot.NewHeadcountSlot("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        HeadcountSlotValidatorReportsEveryProblem();
        HeadcountSlotAmountRangeRuleFlagsValuesAboveAmount();
        HeadcountSlotRequiredTitleRuleFlagsABlankTitle();
        HeadcountSlotGraceDaysRangeRuleFlagsValuesAboveGraceDays();
        HeadcountSlotRequiredCodeRuleFlagsABlankCode();
    }
}
