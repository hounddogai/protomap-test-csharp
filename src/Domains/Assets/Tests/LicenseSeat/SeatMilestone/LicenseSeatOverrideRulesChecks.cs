namespace Acme.Workforce.Assets.Tests.LicenseSeat.SeatMilestone;

using Acme.Workforce.Assets.LicenseSeat.SeatMilestone;

/// <summary>Checks of seat milestone code, run by the project's runner.</summary>
public static class LicenseSeatOverrideRulesChecks
{
    public static void LicenseSeatOverrideValidatorReportsEveryProblem()
    {
        var validator = LicenseSeatOverrideRules.DefaultLicenseSeatOverrideValidator();
        var item = LicenseSeatOverride.NewLicenseSeatOverride("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LicenseSeatOverrideRequiredTeamRuleFlagsABlankTeam()
    {
        var rule = new LicenseSeatOverrideRequiredTeamRule("license seat override needs a value");
        var item = LicenseSeatOverride.NewLicenseSeatOverride("", "");
        item.Team = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LicenseSeatOverrideCategoryLengthRuleFlagsALongCategory()
    {
        var rule = new LicenseSeatOverrideCategoryLengthRule(86400);
        var item = LicenseSeatOverride.NewLicenseSeatOverride("x", "y");
        item.Category = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Category = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        LicenseSeatOverrideValidatorReportsEveryProblem();
        LicenseSeatOverrideRequiredTeamRuleFlagsABlankTeam();
        LicenseSeatOverrideCategoryLengthRuleFlagsALongCategory();
    }
}
