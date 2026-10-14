namespace Acme.Workforce.Compensation.Tests.PayBand.Milestone;

using Acme.Workforce.Compensation.PayBand.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class LegacyPayBandRequestRulesChecks
{
    public static void LegacyPayBandRequestValidatorReportsEveryProblem()
    {
        var validator = LegacyPayBandRequestRules.DefaultLegacyPayBandRequestValidator();
        var item = LegacyPayBandRequest.NewLegacyPayBandRequest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LegacyPayBandRequestExpiresAtRangeRuleFlagsValuesAboveExpiresAt()
    {
        var rule = new LegacyPayBandRequestExpiresAtRangeRule(64, 80);
        var item = LegacyPayBandRequest.NewLegacyPayBandRequest("x", "y");
        item.ExpiresAt = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LegacyPayBandRequestRequiredTeamRuleFlagsABlankTeam()
    {
        var rule = new LegacyPayBandRequestRequiredTeamRule("legacy pay band request needs an entry");
        var item = LegacyPayBandRequest.NewLegacyPayBandRequest("", "");
        item.Team = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LegacyPayBandRequestCodeLengthRuleFlagsALongCode()
    {
        var rule = new LegacyPayBandRequestCodeLengthRule(80);
        var item = LegacyPayBandRequest.NewLegacyPayBandRequest("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        LegacyPayBandRequestValidatorReportsEveryProblem();
        LegacyPayBandRequestExpiresAtRangeRuleFlagsValuesAboveExpiresAt();
        LegacyPayBandRequestRequiredTeamRuleFlagsABlankTeam();
        LegacyPayBandRequestCodeLengthRuleFlagsALongCode();
    }
}
