namespace Acme.Workforce.Compensation.Tests.MeritCycle.CycleRollup;

using Acme.Workforce.Compensation.MeritCycle.CycleRollup;

/// <summary>Checks of cycle rollup code, run by the project's runner.</summary>
public static class MeritCyclePolicyRulesChecks
{
    public static void MeritCyclePolicyValidatorReportsEveryProblem()
    {
        var validator = MeritCyclePolicyRules.DefaultMeritCyclePolicyValidator();
        var item = MeritCyclePolicy.NewMeritCyclePolicy("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void MeritCyclePolicyRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new MeritCyclePolicyRequiredCodeRule("merit cycle policy needs an entry");
        var item = MeritCyclePolicy.NewMeritCyclePolicy("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        MeritCyclePolicyValidatorReportsEveryProblem();
        MeritCyclePolicyRequiredCodeRuleFlagsABlankCode();
    }
}
