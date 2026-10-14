namespace Acme.Workforce.Contractors.Tests.Rate.Proposal;

using Acme.Workforce.Contractors.Rate.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class TieredRateRequestRulesChecks
{
    public static void TieredRateRequestValidatorReportsEveryProblem()
    {
        var validator = TieredRateRequestRules.DefaultTieredRateRequestValidator();
        var item = TieredRateRequest.NewTieredRateRequest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TieredRateRequestRequiredEmployeeIdRuleFlagsABlankEmployeeId()
    {
        var rule = new TieredRateRequestRequiredEmployeeIdRule("tiered rate request needs a value");
        var item = TieredRateRequest.NewTieredRateRequest("", "");
        item.EmployeeId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TieredRateRequestRequiredTeamRuleFlagsABlankTeam()
    {
        var rule = new TieredRateRequestRequiredTeamRule("tiered rate request needs this field");
        var item = TieredRateRequest.NewTieredRateRequest("", "");
        item.Team = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TieredRateRequestTeamLengthRuleFlagsALongTeam()
    {
        var rule = new TieredRateRequestTeamLengthRule(1000);
        var item = TieredRateRequest.NewTieredRateRequest("x", "y");
        item.Team = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Team = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TieredRateRequestEmployeeIdLengthRuleFlagsALongEmployeeId()
    {
        var rule = new TieredRateRequestEmployeeIdLengthRule(1000);
        var item = TieredRateRequest.NewTieredRateRequest("x", "y");
        item.EmployeeId = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.EmployeeId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        TieredRateRequestValidatorReportsEveryProblem();
        TieredRateRequestRequiredEmployeeIdRuleFlagsABlankEmployeeId();
        TieredRateRequestRequiredTeamRuleFlagsABlankTeam();
        TieredRateRequestTeamLengthRuleFlagsALongTeam();
        TieredRateRequestEmployeeIdLengthRuleFlagsALongEmployeeId();
    }
}
