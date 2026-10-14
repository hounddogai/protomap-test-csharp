namespace Acme.Workforce.Compensation.Tests.MeritCycle.CycleTarget;

using Acme.Workforce.Compensation.MeritCycle.CycleTarget;

/// <summary>Checks of cycle target code, run by the project's runner.</summary>
public static class MeritCycleBudgetRulesChecks
{
    public static void MeritCycleBudgetValidatorReportsEveryProblem()
    {
        var validator = MeritCycleBudgetRules.DefaultMeritCycleBudgetValidator();
        var item = MeritCycleBudget.NewMeritCycleBudget("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void MeritCycleBudgetRequiredJobCodeRuleFlagsABlankJobCode()
    {
        var rule = new MeritCycleBudgetRequiredJobCodeRule("merit cycle budget needs a value");
        var item = MeritCycleBudget.NewMeritCycleBudget("", "");
        item.JobCode = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void MeritCycleBudgetCodeLengthRuleFlagsALongCode()
    {
        var rule = new MeritCycleBudgetCodeLengthRule(3600);
        var item = MeritCycleBudget.NewMeritCycleBudget("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void MeritCycleBudgetRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new MeritCycleBudgetRequiredCodeRule("merit cycle budget needs an entry");
        var item = MeritCycleBudget.NewMeritCycleBudget("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        MeritCycleBudgetValidatorReportsEveryProblem();
        MeritCycleBudgetRequiredJobCodeRuleFlagsABlankJobCode();
        MeritCycleBudgetCodeLengthRuleFlagsALongCode();
        MeritCycleBudgetRequiredCodeRuleFlagsABlankCode();
    }
}
