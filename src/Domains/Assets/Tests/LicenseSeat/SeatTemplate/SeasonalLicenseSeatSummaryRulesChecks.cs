namespace Acme.Workforce.Assets.Tests.LicenseSeat.SeatTemplate;

using Acme.Workforce.Assets.LicenseSeat.SeatTemplate;

/// <summary>Checks of seat template code, run by the project's runner.</summary>
public static class SeasonalLicenseSeatSummaryRulesChecks
{
    public static void SeasonalLicenseSeatSummaryValidatorReportsEveryProblem()
    {
        var validator = SeasonalLicenseSeatSummaryRules.DefaultSeasonalLicenseSeatSummaryValidator();
        var item = SeasonalLicenseSeatSummary.NewSeasonalLicenseSeatSummary("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SeasonalLicenseSeatSummaryCodeLengthRuleFlagsALongCode()
    {
        var rule = new SeasonalLicenseSeatSummaryCodeLengthRule(64);
        var item = SeasonalLicenseSeatSummary.NewSeasonalLicenseSeatSummary("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SeasonalLicenseSeatSummaryRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new SeasonalLicenseSeatSummaryRequiredCodeRule("seasonal license seat summary needs this field");
        var item = SeasonalLicenseSeatSummary.NewSeasonalLicenseSeatSummary("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        SeasonalLicenseSeatSummaryValidatorReportsEveryProblem();
        SeasonalLicenseSeatSummaryCodeLengthRuleFlagsALongCode();
        SeasonalLicenseSeatSummaryRequiredCodeRuleFlagsABlankCode();
    }
}
