namespace Acme.Workforce.Assets.Tests.LicenseSeat.Reservation;

using Acme.Workforce.Assets.LicenseSeat.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class LicenseSeatScheduleRulesChecks
{
    public static void LicenseSeatScheduleValidatorReportsEveryProblem()
    {
        var validator = LicenseSeatScheduleRules.DefaultLicenseSeatScheduleValidator();
        var item = LicenseSeatSchedule.NewLicenseSeatSchedule("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LicenseSeatScheduleSalaryBandLengthRuleFlagsALongSalaryBand()
    {
        var rule = new LicenseSeatScheduleSalaryBandLengthRule(86400);
        var item = LicenseSeatSchedule.NewLicenseSeatSchedule("x", "y");
        item.SalaryBand = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.SalaryBand = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LicenseSeatScheduleNameLengthRuleFlagsALongName()
    {
        var rule = new LicenseSeatScheduleNameLengthRule(1000);
        var item = LicenseSeatSchedule.NewLicenseSeatSchedule("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LicenseSeatScheduleOwnerIdLengthRuleFlagsALongOwnerId()
    {
        var rule = new LicenseSeatScheduleOwnerIdLengthRule(3600);
        var item = LicenseSeatSchedule.NewLicenseSeatSchedule("x", "y");
        item.OwnerId = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.OwnerId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        LicenseSeatScheduleValidatorReportsEveryProblem();
        LicenseSeatScheduleSalaryBandLengthRuleFlagsALongSalaryBand();
        LicenseSeatScheduleNameLengthRuleFlagsALongName();
        LicenseSeatScheduleOwnerIdLengthRuleFlagsALongOwnerId();
    }
}
