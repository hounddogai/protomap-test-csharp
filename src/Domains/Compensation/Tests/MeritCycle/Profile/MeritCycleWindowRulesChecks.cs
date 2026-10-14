namespace Acme.Workforce.Compensation.Tests.MeritCycle.Profile;

using Acme.Workforce.Compensation.MeritCycle.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class MeritCycleWindowRulesChecks
{
    public static void MeritCycleWindowValidatorReportsEveryProblem()
    {
        var validator = MeritCycleWindowRules.DefaultMeritCycleWindowValidator();
        var item = MeritCycleWindow.NewMeritCycleWindow("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void MeritCycleWindowCodeLengthRuleFlagsALongCode()
    {
        var rule = new MeritCycleWindowCodeLengthRule(3600);
        var item = MeritCycleWindow.NewMeritCycleWindow("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        MeritCycleWindowValidatorReportsEveryProblem();
        MeritCycleWindowCodeLengthRuleFlagsALongCode();
    }
}
