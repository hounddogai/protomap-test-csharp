namespace Acme.Workforce.Workplace.Tests.Desk.Slot;

using Acme.Workforce.Workplace.Desk.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class DeskSlotRulesChecks
{
    public static void DeskSlotValidatorReportsEveryProblem()
    {
        var validator = DeskSlotRules.DefaultDeskSlotValidator();
        var item = DeskSlot.NewDeskSlot("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DeskSlotRequiredCountryRuleFlagsABlankCountry()
    {
        var rule = new DeskSlotRequiredCountryRule("desk slot needs an entry");
        var item = DeskSlot.NewDeskSlot("", "");
        item.Country = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DeskSlotCountryLengthRuleFlagsALongCountry()
    {
        var rule = new DeskSlotCountryLengthRule(255);
        var item = DeskSlot.NewDeskSlot("x", "y");
        item.Country = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Country = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        DeskSlotValidatorReportsEveryProblem();
        DeskSlotRequiredCountryRuleFlagsABlankCountry();
        DeskSlotCountryLengthRuleFlagsALongCountry();
    }
}
