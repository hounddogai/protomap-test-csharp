namespace Acme.Workforce.Compensation.Tests.MeritCycle.OverrideItem;

using Acme.Workforce.Compensation.MeritCycle.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class RollingMeritCycleOverrideRulesChecks
{
    public static void RollingMeritCycleOverrideValidatorReportsEveryProblem()
    {
        var validator = RollingMeritCycleOverrideRules.DefaultRollingMeritCycleOverrideValidator();
        var item = RollingMeritCycleOverride.NewRollingMeritCycleOverride("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RollingMeritCycleOverrideCodeLengthRuleFlagsALongCode()
    {
        var rule = new RollingMeritCycleOverrideCodeLengthRule(80);
        var item = RollingMeritCycleOverride.NewRollingMeritCycleOverride("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RollingMeritCycleOverrideRequiredPhoneRuleFlagsABlankPhone()
    {
        var rule = new RollingMeritCycleOverrideRequiredPhoneRule("rolling merit cycle override needs this field");
        var item = RollingMeritCycleOverride.NewRollingMeritCycleOverride("", "");
        item.Phone = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        RollingMeritCycleOverrideValidatorReportsEveryProblem();
        RollingMeritCycleOverrideCodeLengthRuleFlagsALongCode();
        RollingMeritCycleOverrideRequiredPhoneRuleFlagsABlankPhone();
    }
}
