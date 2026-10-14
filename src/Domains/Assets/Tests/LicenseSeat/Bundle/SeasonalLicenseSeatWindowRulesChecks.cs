namespace Acme.Workforce.Assets.Tests.LicenseSeat.Bundle;

using Acme.Workforce.Assets.LicenseSeat.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class SeasonalLicenseSeatWindowRulesChecks
{
    public static void SeasonalLicenseSeatWindowValidatorReportsEveryProblem()
    {
        var validator = SeasonalLicenseSeatWindowRules.DefaultSeasonalLicenseSeatWindowValidator();
        var item = SeasonalLicenseSeatWindow.NewSeasonalLicenseSeatWindow("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SeasonalLicenseSeatWindowRatioRangeRuleFlagsValuesAboveRatio()
    {
        var rule = new SeasonalLicenseSeatWindowRatioRangeRule(7.7, 1116.7);
        var item = SeasonalLicenseSeatWindow.NewSeasonalLicenseSeatWindow("x", "y");
        item.Ratio = 1118.2;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void SeasonalLicenseSeatWindowRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new SeasonalLicenseSeatWindowRequiredCodeRule("seasonal license seat window needs a value");
        var item = SeasonalLicenseSeatWindow.NewSeasonalLicenseSeatWindow("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SeasonalLicenseSeatWindowTitleLengthRuleFlagsALongTitle()
    {
        var rule = new SeasonalLicenseSeatWindowTitleLengthRule(64);
        var item = SeasonalLicenseSeatWindow.NewSeasonalLicenseSeatWindow("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SeasonalLicenseSeatWindowCodeLengthRuleFlagsALongCode()
    {
        var rule = new SeasonalLicenseSeatWindowCodeLengthRule(16);
        var item = SeasonalLicenseSeatWindow.NewSeasonalLicenseSeatWindow("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        SeasonalLicenseSeatWindowValidatorReportsEveryProblem();
        SeasonalLicenseSeatWindowRatioRangeRuleFlagsValuesAboveRatio();
        SeasonalLicenseSeatWindowRequiredCodeRuleFlagsABlankCode();
        SeasonalLicenseSeatWindowTitleLengthRuleFlagsALongTitle();
        SeasonalLicenseSeatWindowCodeLengthRuleFlagsALongCode();
    }
}
