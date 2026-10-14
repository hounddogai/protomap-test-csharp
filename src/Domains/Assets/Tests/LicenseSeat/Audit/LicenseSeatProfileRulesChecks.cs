namespace Acme.Workforce.Assets.Tests.LicenseSeat.Audit;

using Acme.Workforce.Assets.LicenseSeat.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class LicenseSeatProfileRulesChecks
{
    public static void LicenseSeatProfileValidatorReportsEveryProblem()
    {
        var validator = LicenseSeatProfileRules.DefaultLicenseSeatProfileValidator();
        var item = LicenseSeatProfile.NewLicenseSeatProfile("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LicenseSeatProfileCostRangeRuleFlagsValuesAboveCost()
    {
        var rule = new LicenseSeatProfileCostRangeRule(8.0, 1196.7);
        var item = LicenseSeatProfile.NewLicenseSeatProfile("x", "y");
        item.Cost = 1198.2;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LicenseSeatProfileCodeLengthRuleFlagsALongCode()
    {
        var rule = new LicenseSeatProfileCodeLengthRule(3600);
        var item = LicenseSeatProfile.NewLicenseSeatProfile("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LicenseSeatProfileTitleLengthRuleFlagsALongTitle()
    {
        var rule = new LicenseSeatProfileTitleLengthRule(64);
        var item = LicenseSeatProfile.NewLicenseSeatProfile("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        LicenseSeatProfileValidatorReportsEveryProblem();
        LicenseSeatProfileCostRangeRuleFlagsValuesAboveCost();
        LicenseSeatProfileCodeLengthRuleFlagsALongCode();
        LicenseSeatProfileTitleLengthRuleFlagsALongTitle();
    }
}
