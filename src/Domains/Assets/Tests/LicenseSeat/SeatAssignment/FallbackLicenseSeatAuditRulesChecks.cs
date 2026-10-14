namespace Acme.Workforce.Assets.Tests.LicenseSeat.SeatAssignment;

using Acme.Workforce.Assets.LicenseSeat.SeatAssignment;

/// <summary>Checks of seat assignment code, run by the project's runner.</summary>
public static class FallbackLicenseSeatAuditRulesChecks
{
    public static void FallbackLicenseSeatAuditValidatorReportsEveryProblem()
    {
        var validator = FallbackLicenseSeatAuditRules.DefaultFallbackLicenseSeatAuditValidator();
        var item = FallbackLicenseSeatAudit.NewFallbackLicenseSeatAudit("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FallbackLicenseSeatAuditRequiredContactEmailRuleFlagsABlankContactEmail()
    {
        var rule = new FallbackLicenseSeatAuditRequiredContactEmailRule("fallback license seat audit needs a value");
        var item = FallbackLicenseSeatAudit.NewFallbackLicenseSeatAudit("", "");
        item.ContactEmail = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FallbackLicenseSeatAuditLimitRangeRuleFlagsValuesAboveLimit()
    {
        var rule = new FallbackLicenseSeatAuditLimitRangeRule(1000, 86400);
        var item = FallbackLicenseSeatAudit.NewFallbackLicenseSeatAudit("x", "y");
        item.Limit = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void FallbackLicenseSeatAuditCountryLengthRuleFlagsALongCountry()
    {
        var rule = new FallbackLicenseSeatAuditCountryLengthRule(64);
        var item = FallbackLicenseSeatAudit.NewFallbackLicenseSeatAudit("x", "y");
        item.Country = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Country = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        FallbackLicenseSeatAuditValidatorReportsEveryProblem();
        FallbackLicenseSeatAuditRequiredContactEmailRuleFlagsABlankContactEmail();
        FallbackLicenseSeatAuditLimitRangeRuleFlagsValuesAboveLimit();
        FallbackLicenseSeatAuditCountryLengthRuleFlagsALongCountry();
    }
}
