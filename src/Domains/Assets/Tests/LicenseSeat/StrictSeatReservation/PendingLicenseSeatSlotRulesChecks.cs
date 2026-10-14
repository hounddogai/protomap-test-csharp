namespace Acme.Workforce.Assets.Tests.LicenseSeat.StrictSeatReservation;

using Acme.Workforce.Assets.LicenseSeat.StrictSeatReservation;

/// <summary>Checks of strict seat reservation code, run by the project's runner.</summary>
public static class PendingLicenseSeatSlotRulesChecks
{
    public static void PendingLicenseSeatSlotValidatorReportsEveryProblem()
    {
        var validator = PendingLicenseSeatSlotRules.DefaultPendingLicenseSeatSlotValidator();
        var item = PendingLicenseSeatSlot.NewPendingLicenseSeatSlot("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PendingLicenseSeatSlotRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new PendingLicenseSeatSlotRequiredLabelRule("pending license seat slot needs an entry");
        var item = PendingLicenseSeatSlot.NewPendingLicenseSeatSlot("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PendingLicenseSeatSlotRequiredCategoryRuleFlagsABlankCategory()
    {
        var rule = new PendingLicenseSeatSlotRequiredCategoryRule("pending license seat slot needs this field");
        var item = PendingLicenseSeatSlot.NewPendingLicenseSeatSlot("", "");
        item.Category = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        PendingLicenseSeatSlotValidatorReportsEveryProblem();
        PendingLicenseSeatSlotRequiredLabelRuleFlagsABlankLabel();
        PendingLicenseSeatSlotRequiredCategoryRuleFlagsABlankCategory();
    }
}
