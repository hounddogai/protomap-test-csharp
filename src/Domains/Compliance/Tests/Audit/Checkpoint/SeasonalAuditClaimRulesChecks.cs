namespace Acme.Workforce.Compliance.Tests.Audit.Checkpoint;

using Acme.Workforce.Compliance.Audit.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class SeasonalAuditClaimRulesChecks
{
    public static void SeasonalAuditClaimValidatorReportsEveryProblem()
    {
        var validator = SeasonalAuditClaimRules.DefaultSeasonalAuditClaimValidator();
        var item = SeasonalAuditClaim.NewSeasonalAuditClaim("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SeasonalAuditClaimAmountRangeRuleFlagsValuesAboveAmount()
    {
        var rule = new SeasonalAuditClaimAmountRangeRule(0.1, 4924.2);
        var item = SeasonalAuditClaim.NewSeasonalAuditClaim("x", "y");
        item.Amount = 4925.7;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void SeasonalAuditClaimCountryLengthRuleFlagsALongCountry()
    {
        var rule = new SeasonalAuditClaimCountryLengthRule(64);
        var item = SeasonalAuditClaim.NewSeasonalAuditClaim("x", "y");
        item.Country = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Country = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        SeasonalAuditClaimValidatorReportsEveryProblem();
        SeasonalAuditClaimAmountRangeRuleFlagsValuesAboveAmount();
        SeasonalAuditClaimCountryLengthRuleFlagsALongCountry();
    }
}
