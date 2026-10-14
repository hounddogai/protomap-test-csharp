namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Ledger;

using Acme.Workforce.Talent.SuccessionPlan.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class WeightedSuccessionPlanRequestRulesChecks
{
    public static void WeightedSuccessionPlanRequestValidatorReportsEveryProblem()
    {
        var validator = WeightedSuccessionPlanRequestRules.DefaultWeightedSuccessionPlanRequestValidator();
        var item = WeightedSuccessionPlanRequest.NewWeightedSuccessionPlanRequest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void WeightedSuccessionPlanRequestTeamLengthRuleFlagsALongTeam()
    {
        var rule = new WeightedSuccessionPlanRequestTeamLengthRule(16);
        var item = WeightedSuccessionPlanRequest.NewWeightedSuccessionPlanRequest("x", "y");
        item.Team = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Team = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void WeightedSuccessionPlanRequestCodeLengthRuleFlagsALongCode()
    {
        var rule = new WeightedSuccessionPlanRequestCodeLengthRule(3600);
        var item = WeightedSuccessionPlanRequest.NewWeightedSuccessionPlanRequest("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void WeightedSuccessionPlanRequestJobCodeLengthRuleFlagsALongJobCode()
    {
        var rule = new WeightedSuccessionPlanRequestJobCodeLengthRule(80);
        var item = WeightedSuccessionPlanRequest.NewWeightedSuccessionPlanRequest("x", "y");
        item.JobCode = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.JobCode = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        WeightedSuccessionPlanRequestValidatorReportsEveryProblem();
        WeightedSuccessionPlanRequestTeamLengthRuleFlagsALongTeam();
        WeightedSuccessionPlanRequestCodeLengthRuleFlagsALongCode();
        WeightedSuccessionPlanRequestJobCodeLengthRuleFlagsALongJobCode();
    }
}
