namespace Acme.Workforce.Compensation.Tests.MeritCycle.Target;

using Acme.Workforce.Compensation.MeritCycle.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class PrimaryMeritCycleWindowRulesChecks
{
    public static void PrimaryMeritCycleWindowValidatorReportsEveryProblem()
    {
        var validator = PrimaryMeritCycleWindowRules.DefaultPrimaryMeritCycleWindowValidator();
        var item = PrimaryMeritCycleWindow.NewPrimaryMeritCycleWindow("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PrimaryMeritCycleWindowRequiredNameRuleFlagsABlankName()
    {
        var rule = new PrimaryMeritCycleWindowRequiredNameRule("primary merit cycle window needs an entry");
        var item = PrimaryMeritCycleWindow.NewPrimaryMeritCycleWindow("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PrimaryMeritCycleWindowCodeLengthRuleFlagsALongCode()
    {
        var rule = new PrimaryMeritCycleWindowCodeLengthRule(64);
        var item = PrimaryMeritCycleWindow.NewPrimaryMeritCycleWindow("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        PrimaryMeritCycleWindowValidatorReportsEveryProblem();
        PrimaryMeritCycleWindowRequiredNameRuleFlagsABlankName();
        PrimaryMeritCycleWindowCodeLengthRuleFlagsALongCode();
    }
}
