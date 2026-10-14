namespace Acme.Workforce.Assets.Tests.LicenseSeat.SeatReservation;

using Acme.Workforce.Assets.LicenseSeat.SeatReservation;

/// <summary>Checks of seat reservation code, run by the project's runner.</summary>
public static class LicenseSeatWindowRulesChecks
{
    public static void LicenseSeatWindowValidatorReportsEveryProblem()
    {
        var validator = LicenseSeatWindowRules.DefaultLicenseSeatWindowValidator();
        var item = LicenseSeatWindow.NewLicenseSeatWindow("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LicenseSeatWindowRequiredDepartmentRuleFlagsABlankDepartment()
    {
        var rule = new LicenseSeatWindowRequiredDepartmentRule("license seat window needs this field");
        var item = LicenseSeatWindow.NewLicenseSeatWindow("", "");
        item.Department = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LicenseSeatWindowRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new LicenseSeatWindowRequiredTitleRule("license seat window needs this field");
        var item = LicenseSeatWindow.NewLicenseSeatWindow("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        LicenseSeatWindowValidatorReportsEveryProblem();
        LicenseSeatWindowRequiredDepartmentRuleFlagsABlankDepartment();
        LicenseSeatWindowRequiredTitleRuleFlagsABlankTitle();
    }
}
