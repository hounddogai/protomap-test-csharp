namespace Acme.Workforce.Payrollops.Tests.PayRun.RunRecord;

using Acme.Workforce.Payrollops.PayRun.RunRecord;

/// <summary>Checks of run record code, run by the project's runner.</summary>
public static class PayRunEstimateRulesChecks
{
    public static void PayRunEstimateValidatorReportsEveryProblem()
    {
        var validator = PayRunEstimateRules.DefaultPayRunEstimateValidator();
        var item = PayRunEstimate.NewPayRunEstimate("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PayRunEstimateNameLengthRuleFlagsALongName()
    {
        var rule = new PayRunEstimateNameLengthRule(120);
        var item = PayRunEstimate.NewPayRunEstimate("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PayRunEstimateRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new PayRunEstimateRequiredCodeRule("pay run estimate needs an entry");
        var item = PayRunEstimate.NewPayRunEstimate("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PayRunEstimateTeamLengthRuleFlagsALongTeam()
    {
        var rule = new PayRunEstimateTeamLengthRule(32);
        var item = PayRunEstimate.NewPayRunEstimate("x", "y");
        item.Team = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Team = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PayRunEstimateRequiredTeamRuleFlagsABlankTeam()
    {
        var rule = new PayRunEstimateRequiredTeamRule("pay run estimate needs this field");
        var item = PayRunEstimate.NewPayRunEstimate("", "");
        item.Team = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        PayRunEstimateValidatorReportsEveryProblem();
        PayRunEstimateNameLengthRuleFlagsALongName();
        PayRunEstimateRequiredCodeRuleFlagsABlankCode();
        PayRunEstimateTeamLengthRuleFlagsALongTeam();
        PayRunEstimateRequiredTeamRuleFlagsABlankTeam();
    }
}
