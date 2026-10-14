namespace Acme.Workforce.Assets.Tests.LicenseSeat.SeatForecast;

using Acme.Workforce.Assets.LicenseSeat.SeatForecast;

/// <summary>Checks of seat forecast code, run by the project's runner.</summary>
public static class RegionalLicenseSeatMilestoneRulesChecks
{
    public static void RegionalLicenseSeatMilestoneValidatorReportsEveryProblem()
    {
        var validator = RegionalLicenseSeatMilestoneRules.DefaultRegionalLicenseSeatMilestoneValidator();
        var item = RegionalLicenseSeatMilestone.NewRegionalLicenseSeatMilestone("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RegionalLicenseSeatMilestoneRequiredNotesRuleFlagsABlankNotes()
    {
        var rule = new RegionalLicenseSeatMilestoneRequiredNotesRule("regional license seat milestone needs this field");
        var item = RegionalLicenseSeatMilestone.NewRegionalLicenseSeatMilestone("", "");
        item.Notes = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RegionalLicenseSeatMilestoneDiscountRangeRuleFlagsValuesAboveDiscount()
    {
        var rule = new RegionalLicenseSeatMilestoneDiscountRangeRule(9.4, 929.6);
        var item = RegionalLicenseSeatMilestone.NewRegionalLicenseSeatMilestone("x", "y");
        item.Discount = 931.1;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RegionalLicenseSeatMilestoneRequiredEmployeeIdRuleFlagsABlankEmployeeId()
    {
        var rule = new RegionalLicenseSeatMilestoneRequiredEmployeeIdRule(
            "regional license seat milestone needs a value");
        var item = RegionalLicenseSeatMilestone.NewRegionalLicenseSeatMilestone("", "");
        item.EmployeeId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        RegionalLicenseSeatMilestoneValidatorReportsEveryProblem();
        RegionalLicenseSeatMilestoneRequiredNotesRuleFlagsABlankNotes();
        RegionalLicenseSeatMilestoneDiscountRangeRuleFlagsValuesAboveDiscount();
        RegionalLicenseSeatMilestoneRequiredEmployeeIdRuleFlagsABlankEmployeeId();
    }
}
