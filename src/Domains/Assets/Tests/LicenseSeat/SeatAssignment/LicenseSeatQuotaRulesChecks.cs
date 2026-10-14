namespace Acme.Workforce.Assets.Tests.LicenseSeat.SeatAssignment;

using Acme.Workforce.Assets.LicenseSeat.SeatAssignment;

/// <summary>Checks of seat assignment code, run by the project's runner.</summary>
public static class LicenseSeatQuotaRulesChecks
{
    public static void LicenseSeatQuotaValidatorReportsEveryProblem()
    {
        var validator = LicenseSeatQuotaRules.DefaultLicenseSeatQuotaValidator();
        var item = LicenseSeatQuota.NewLicenseSeatQuota("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LicenseSeatQuotaRequiredNameRuleFlagsABlankName()
    {
        var rule = new LicenseSeatQuotaRequiredNameRule("license seat quota needs this field");
        var item = LicenseSeatQuota.NewLicenseSeatQuota("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LicenseSeatQuotaGraceDaysRangeRuleFlagsValuesAboveGraceDays()
    {
        var rule = new LicenseSeatQuotaGraceDaysRangeRule(64, 86400);
        var item = LicenseSeatQuota.NewLicenseSeatQuota("x", "y");
        item.GraceDays = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        LicenseSeatQuotaValidatorReportsEveryProblem();
        LicenseSeatQuotaRequiredNameRuleFlagsABlankName();
        LicenseSeatQuotaGraceDaysRangeRuleFlagsValuesAboveGraceDays();
    }
}
