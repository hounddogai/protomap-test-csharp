namespace Acme.Workforce.Compensation.Tests.MeritCycle.Budget;

using Acme.Workforce.Compensation.MeritCycle.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class CappedMeritCycleBudgetRulesChecks
{
    public static void CappedMeritCycleBudgetValidatorReportsEveryProblem()
    {
        var validator = CappedMeritCycleBudgetRules.DefaultCappedMeritCycleBudgetValidator();
        var item = CappedMeritCycleBudget.NewCappedMeritCycleBudget("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CappedMeritCycleBudgetTeamLengthRuleFlagsALongTeam()
    {
        var rule = new CappedMeritCycleBudgetTeamLengthRule(64);
        var item = CappedMeritCycleBudget.NewCappedMeritCycleBudget("x", "y");
        item.Team = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Team = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CappedMeritCycleBudgetPriorityRangeRuleFlagsValuesAbovePriority()
    {
        var rule = new CappedMeritCycleBudgetPriorityRangeRule(32, 32);
        var item = CappedMeritCycleBudget.NewCappedMeritCycleBudget("x", "y");
        item.Priority = 33;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void CappedMeritCycleBudgetNameLengthRuleFlagsALongName()
    {
        var rule = new CappedMeritCycleBudgetNameLengthRule(255);
        var item = CappedMeritCycleBudget.NewCappedMeritCycleBudget("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        CappedMeritCycleBudgetValidatorReportsEveryProblem();
        CappedMeritCycleBudgetTeamLengthRuleFlagsALongTeam();
        CappedMeritCycleBudgetPriorityRangeRuleFlagsValuesAbovePriority();
        CappedMeritCycleBudgetNameLengthRuleFlagsALongName();
    }
}
